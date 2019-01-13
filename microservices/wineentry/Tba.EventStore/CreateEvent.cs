using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tba.EventStore
{
    public static class CreateEVent
    {
        private const string Method = "post";
        private const string Route = "events";
        private const string OperationName = "TBA-create-application-event";
        private const string CorrelationIdHeaderKey = "Request-Id";
        public const string EventTrigger = "Http";
        public const string EntityType = "ApplicationEvent";
        private const int MethodEventBase = 0;

        private enum ProcessStep
        {
            Deserialize = 1,
            Validation = 2,
            SequenceValidation = 3,
            CreateDocument = 4
        }

        private enum DeserializeMessage
        {
            NullRequestBody
        }

        private enum ValidateMessage
        {
            InvalidRequestBody
        }

        public enum CreateDocumentMessage
        {
            DocumentNull,
            Aggregate
        }

        [FunctionName(OperationName)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method, Route = Route)]HttpRequestMessage request,
            [CosmosDBTrigger(StaticSettings.Db, StaticSettings.Collection,
                ConnectionStringSetting = StaticSettings.DbConnectionStringSetting)]IDocumentClient documentClient,
            ISupportNotifier supportNotifier,
            ILogger logger)
        {
            // correlation
            request.Headers.TryGetValues(CorrelationIdHeaderKey, out var headerValues);
            var correlationId = headerValues == null ? Guid.NewGuid().ToString() : headerValues.First();

            // deserialize
            Event eventModel;
            string payload = null;
            var eventId = StaticSettings.EventId(MethodEventBase, (int)ProcessStep.Deserialize);
            try
            {
                payload = await request.Content.ReadAsStringAsync();
                eventModel = JsonConvert.DeserializeObject<Event>(payload);
                logger.LogInformation(eventId, StaticSettings.Template, EventTrigger, correlationId,
                    EntityType, null, ProcessStep.Deserialize.ToString());
            }
            catch (ArgumentNullException)
            {
                return BadRequest(logger, eventId, correlationId, ProcessStep.Deserialize, DeserializeMessage.NullRequestBody.ToString());
            }
            catch (ArgumentException ex)
            {
                return BadRequest(logger, eventId, correlationId, ProcessStep.Deserialize, ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest(logger, supportNotifier, eventId, correlationId, ProcessStep.Deserialize, e, payload);
            }

            // validate
            eventId = StaticSettings.EventId(MethodEventBase, (int)ProcessStep.Validation);
            var docUri = (UriFactory.CreateDocumentCollectionUri(StaticSettings.Db, StaticSettings.Collection);
            try
            {
                Validator.ValidateObject(eventModel, new ValidationContext(eventModel));
                logger.LogInformation(eventId, StaticSettings.Template, EventTrigger, correlationId,
                    EntityType, eventModel.EventId, ProcessStep.Validation.ToString());
            }
            catch (ValidationException)
            {
                return BadRequest(logger, eventId, correlationId, ProcessStep.Validation,
                    ValidateMessage.InvalidRequestBody.ToString());
            }
            catch (Exception e)
            {
                return BadRequest(logger, supportNotifier, eventId, correlationId, ProcessStep.Validation, e, payload);
            }

            // check for valid event sequence of the event in our event store
            // This may result in a stale sequence number, but we catch that in the "create document" step
            int sequence = -1;
            try
            {
                sequence = documentClient.CreateDocumentQuery<int>(docUri, MaxSequence(eventModel.AggregateId))
                    .AsEnumerable().FirstOrDefault();

            }
            catch (DocumentClientException ex)
            {
                return BadRequest(logger, supportNotifier, eventId, correlationId, ex, payload);
            }
            catch (Exception ex)
            {
                return BadRequest(logger, supportNotifier, eventId, correlationId, ProcessStep.CreateDocument, ex, payload);
            }


            // Create document
            eventId = StaticSettings.EventId(MethodEventBase, (int)ProcessStep.CreateDocument);
            try
            {
                await documentClient.CreateDocumentAsync(docUri, eventModel);
                logger.LogInformation(eventId, StaticSettings.Template, EventTrigger, correlationId,
                    EntityType, eventModel.EventId, ProcessStep.CreateDocument.ToString());
                return new OkResult();
            }
            // If either documentsFeedOrDatabaseLink or document is not set.
            catch (ArgumentNullException)
            {
                return BadRequest(logger, eventId, correlationId, ProcessStep.CreateDocument, CreateDocumentMessage.DocumentNull.ToString());
            }
            // Represents a consolidation of failures that occured during async processing.
            // Look within InnerExceptions to find the actual exception(s)
            catch (AggregateException)
            {
                // todo: log inner exceptions individually
                return BadRequest(logger, eventId, correlationId, ProcessStep.CreateDocument, CreateDocumentMessage.Aggregate.ToString());
            }
            catch (DocumentClientException ex)
            {
                return BadRequest(logger, supportNotifier, eventId, correlationId, ex, payload);
            }
            catch (Exception ex)
            {
                return BadRequest(logger, supportNotifier, eventId, correlationId, ProcessStep.CreateDocument, ex, payload);
            }
        }



        /// <summary>
        /// Log an error for the unhandled exception
        /// Notify support of this error
        /// </summary>
        private static BadRequestObjectResult BadRequest(ILogger logger, ISupportNotifier supportNotifier,
            EventId eventId, string correlationId, ProcessStep processStep, Exception ex, string payload)
        {
            var message = $"{processStep.ToString()} {ex.Message}";
            // log exception when unknown exception
            logger.LogError(eventId, ex, StaticSettings.Template, EventTrigger, correlationId, EntityType, null, message);
            // notify support of unknown exception
            supportNotifier.Notify($"WJ Unhandled Exception. Operation: {OperationName}, Step: {processStep.ToString()}, Request body: {payload}, Message: {message}");
            // return bad request with unknown exception
            return new BadRequestObjectResult(new JObject()["error"] = "unknown error");
        }

        /// <summary>
        /// Log an error for the document client exception
        /// Notify support of this error
        /// 
        /// This exception can encapsulate many different types of errors. To determine the specific error
        /// always look at the StatusCode property. Some common codes you may get when creating a Document are:
        /// 400 BadRequest - This means something was wrong with the document supplied.It is likely that
        ///                  disableAutomaticIdGeneration was true and an id was not supplied
        /// 403 Forbidden - This likely means the collection in to which you were trying to create the document is full.
        /// 409 Conflict - This means a Document with an id matching the id field of document already existed
        /// 413 RequestEntityTooLarge - This means the Document exceeds the current max entity size.Consult
        ///                             documentation for limits and quotas.
        /// 429 TooManyRequests - This means you have exceeded the number of request units per second.Consult the
        ///                       DocumentClientException.RetryAfter value to see how long you should wait before
        ///                       retrying this operation.
        /// </summary>
        private static BadRequestObjectResult BadRequest(ILogger logger, ISupportNotifier supportNotifier,
            EventId eventId, string correlationId, DocumentClientException ex, string payload)
        {
            var message = $"{ProcessStep.CreateDocument.ToString()}, DocumentClientException status: {ex.StatusCode}, message: {ex.Message}";
            // log exception when unknown exception
            logger.LogError(eventId, ex, StaticSettings.Template, EventTrigger, correlationId, EntityType, null, message);
            // notify support of unknown exception
            supportNotifier.Notify($"WJ Unhandled Exception. Operation: {OperationName}, Step: {ProcessStep.CreateDocument.ToString()}, Request body: {payload}, Message: {message}");
            // return bad request with unknown exception
            return new BadRequestObjectResult(new JObject()["error"] = "unknown error");
        }

        /// <summary>
        /// Log a warning of a handled exception
        /// </summary>
        private static BadRequestObjectResult BadRequest(ILogger logger, EventId eventId, string correlationId, ProcessStep processStep, string message)
        {
            logger.LogWarning(eventId, StaticSettings.Template, EventTrigger, correlationId, EntityType, null, $"{processStep.ToString()} {message}");
            return new BadRequestObjectResult(new JObject()["error"] = message);
        }

        private static SqlQuerySpec MaxSequence(Guid aggregateId)
        {
            return new SqlQuerySpec
            {
                QueryText = "SELECT VALUE MAX(e.sequence) FROM events e WHERE (e.aggregateId = @aggregateId)",
                Parameters = new SqlParameterCollection()
                {
                    new SqlParameter("@aggregateId", aggregateId.ToString())
                }
            };
        }
    }
}
