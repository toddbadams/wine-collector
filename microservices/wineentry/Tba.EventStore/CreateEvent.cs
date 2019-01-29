using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;

namespace Tba.EventStore
{
    /// <summary>
    /// 
    /// </summary>
    public static class CreateEVent
    {
        private const string Method = "post";
        private const string Route = "events";
        private const string OperationName = "TBA-create-application-event";
        private const string CorrelationIdHeaderKey = "Request-Id";
        private const string Topic = "";
        private const string ServiceBusSendConnectionStringSetting = "";
        private const string Template = "WJ {Operation}, {Description}, {CorrelationId}, {EntityType}, {EntityId}";
        private const int ServiceLevelEventId = 21000;
        private const int MethodLevelEventId = 100;
        private const char Separator = '|';


        private enum ProcessStep
        {
            Deserialize = 1,
            Validation = 2,
            CreateEventMessage = 3,
            PublishMessage = 4
        }

        [FunctionName(OperationName)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method, Route = Route)]HttpRequestMessage request,
            [ServiceBus(Topic, Connection = ServiceBusSendConnectionStringSetting)]IAsyncCollector<Message> messageAsyncCollector,
            ILogger logger)
        {
            // get the correlation identifier
            request.Headers.TryGetValues(CorrelationIdHeaderKey, out var headerValues);
            var correlationId = headerValues == null ? Guid.NewGuid().ToString() : headerValues.First();

            // ProcessStep.Deserialize - deserialize request content
            EventRequest eventRequest;
            string payload = null;
            var eventId = EventId(ProcessStep.Deserialize);
            try
            {
                payload = await request.Content.ReadAsStringAsync();
                eventRequest = JsonConvert.DeserializeObject<EventRequest>(payload);
                logger.LogInformation(eventId, Template, OperationName, correlationId);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(logger, eventId, correlationId, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(logger, eventId, correlationId, ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(logger, eventId, correlationId, payload, ex);
            }

            // ProcessStep.Validation - validate the event request
            eventId = EventId(ProcessStep.CreateEventMessage);
            try
            {
                Validator.ValidateObject(eventRequest, new ValidationContext(eventRequest));
                logger.LogInformation(eventId, Template, OperationName, correlationId);
            }
            catch (ValidationException)
            {
                return BadRequest(logger, eventId, correlationId, "Invalid request content");
            }
            catch (Exception ex)
            {
                return BadRequest(logger, eventId, correlationId, payload, ex);
            }

            // ProcessStep.CreateEventMessage - create the event message
            var message = CreateMessage(eventRequest, correlationId);
            logger.LogInformation(EventId(ProcessStep.Validation), Template, OperationName, correlationId);

            // ProcessStep.PublishMessage - publish the event message
            eventId = EventId(ProcessStep.PublishMessage);
            try
            {
                await messageAsyncCollector.AddAsync(message);
                logger.LogInformation(eventId, Template, OperationName, correlationId);
            }
            catch (Exception ex)
            {
                return BadRequest(logger, eventId, correlationId, payload, ex);
            }

            return new OkObjectResult(new JObject());
        }

        /// <summary>
        /// Handled exception
        /// </summary>
        private static BadRequestObjectResult BadRequest(ILogger logger, EventId eventId, string correlationId, string message)
        {
            logger.LogWarning(eventId, Template, OperationName, correlationId, message);
            return new BadRequestObjectResult(new JObject()["error"] = message);
        }

        /// <summary>
        /// Unhandled exception
        /// </summary>
        private static BadRequestObjectResult BadRequest(ILogger logger, EventId eventId, string correlationId, string payload, Exception ex)
        {
            var message =
                $"Unhandled Exception. Operation: {OperationName}, Step: {eventId.Name}, Request body: {payload}, Message: {ex.Message}";
            // log exception
            logger.LogError(eventId, ex, Template, OperationName, correlationId, message);
            // return bad request with unknown exception
            return new BadRequestObjectResult(new JObject()["error"] = "unknown error");
        }

        /// <summary>
        /// The sequence number is a unique 64-bit integer assigned to a message as it is accepted
        /// and stored by the broker and functions as its true identifier.
        /// For partitioned entities, the topmost 16 bits reflect the partition identifier.
        /// Sequence numbers monotonically increase and are gap-less.
        /// They roll over to 0 when the 48-64 bit range is exhausted.
        /// This property is read-only.
        /// </summary>
        /// <returns></returns>
        private static Message CreateMessage(EventRequest request, string correlationId)
        {
            return new Message(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(request.Value)))
            {
                ContentType = "application/json",
                CorrelationId = correlationId,
                Label = $"{request.EventTypeVersion}{Separator}{request.EventTypeVersion}",
                MessageId = $"{request.AggregateId}{Separator}{Guid.NewGuid()}",
                PartitionKey = request.AggregateType
            };
        }

        /// <summary>
        /// Create an EventId used for logging
        /// </summary>
        private static EventId EventId(ProcessStep step) =>
            new EventId(ServiceLevelEventId + MethodLevelEventId + (int)step, step.ToString());
    }
}
