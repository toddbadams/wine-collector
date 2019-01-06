using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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
using Tba.WineEntry;

namespace Tba.EventStore
{
    public static class CreateEVent
    {
        private const string Method = "post";
        private const string Route = "events";
        private const string CorrelationIdHeaderKey = "Request-Id";
        private const string Template = "WineJargon {Trigger}, {CorrelationId}, {EntityType}, {EntityId}, {Description}";
        private const int EventBase = 21000;
        public const string Db = "event-store";
        public const string Collection = "events";
        public const string LeasesCollection = "event-leases";
        public const string DbConnectionStringSetting = "CosmosConnectionString";
        public const string Topic = "";
        public const string ServiceBusSendConnectionStringSetting = "";
        public const string EventTrigger = "Http";
        public const string EntityType = "Event";

        private enum EventType
        {
            Deserialize = 1,
            Validation = 2,
            CreateDocument = 3,
            ChangeFeed = 4
        }

        private enum DeserializeMessage
        {
            NullRequestBody
        }

        private enum ValidateMessage
        {
            Success,
            InvalidRequestBody
        }

        public enum DocumentClientMessage
        {
            Success,
            DocumentNull,
            Aggregate,
            InvalidDocument,
            CollectionForbidden,
            ConflictWithExistingDocument,
            RequestEntityTooLarge,
            TooManyRequests,
            Unknown
        }

        [FunctionName("CreateEvent")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, Method, Route = Route)]HttpRequestMessage request,
            [CosmosDBTrigger(Db, Collection, ConnectionStringSetting = DbConnectionStringSetting)]IDocumentClient documentClient,
            ILogger logger)
        {
            // correlation
            request.Headers.TryGetValues(CorrelationIdHeaderKey, out var headerValues);
            var correlationId  =  headerValues == null ? Guid.NewGuid().ToString() : headerValues.First();

            Event eventModel;
            // deserialize
            try
            {
                eventModel = JsonConvert.DeserializeObject<Event>(await request.Content.ReadAsStringAsync());
                logger.LogInformation(EventId(EventType.Deserialize), Template, EventTrigger, correlationId,
                    EntityType, null, EventType.Deserialize.ToString());
            }
            catch (ArgumentNullException)
            {
                return BadRequest(logger, correlationId, EventType.Deserialize, DeserializeMessage.NullRequestBody.ToString());
            }
            catch (ArgumentException ex)
            {
                return BadRequest(logger, correlationId, EventType.Deserialize, ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest(logger, correlationId, EventType.Deserialize, null, e);
            }

            // validate
            try
            {
                Validator.ValidateObject(eventModel, new ValidationContext(eventModel));
                logger.LogInformation(EventId(EventType.Validation), Template, EventTrigger, correlationId,
                    EntityType, eventModel.EventId, EventType.Validation.ToString());
            }
            catch (ValidationException)
            {
                return BadRequest(logger, correlationId, EventType.Validation,
                    ValidateMessage.InvalidRequestBody.ToString());
            }
            catch (Exception e)
            {
                return BadRequest(logger, correlationId, EventType.Validation, null, e);
            }

            // Create document
            try
            {
                await documentClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(Db, Collection),
                        eventModel);
                logger.LogInformation(EventId(EventType.CreateDocument), Template, EventTrigger, correlationId,
                    EntityType, eventModel.EventId, EventType.CreateDocument.ToString());
                return new OkResult();
            }
            // If either documentsFeedOrDatabaseLink or document is not set.
            catch (ArgumentNullException)
            {
                return BadRequest(logger, correlationId, EventType.CreateDocument,
                    DocumentClientMessage.DocumentNull.ToString());
            }
            // Represents a consolidation of failures that occured during async processing.
            // Look within InnerExceptions to find the actual exception(s)
            catch (AggregateException)
            {
                return BadRequest(logger, correlationId, EventType.CreateDocument,
                    DocumentClientMessage.Aggregate.ToString());
            }
            // This exception can encapsulate many different types of errors. To determine the specific error
            // always look at the StatusCode property. Some common codes you may get when creating a Document are:
            // 400 BadRequest - This means something was wrong with the document supplied.It is likely that
            //                  disableAutomaticIdGeneration was true and an id was not supplied
            // 403 Forbidden - This likely means the collection in to which you were trying to create the document is full.
            // 409 Conflict - This means a Document with an id matching the id field of document already existed
            // 413 RequestEntityTooLarge - This means the Document exceeds the current max entity size.Consult
            //                             documentation for limits and quotas.
            // 429 TooManyRequests - This means you have exceeded the number of request units per second.Consult the
            //                       DocumentClientException.RetryAfter value to see how long you should wait before
            //                       retrying this operation.
            catch (DocumentClientException ex)
            {
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        return BadRequest(logger, correlationId, EventType.CreateDocument,
                            DocumentClientMessage.InvalidDocument.ToString());

                    case HttpStatusCode.Forbidden:
                        return BadRequest(logger, correlationId, EventType.CreateDocument,
                            DocumentClientMessage.CollectionForbidden.ToString());

                    case HttpStatusCode.Conflict:
                        return BadRequest(logger, correlationId, EventType.CreateDocument,
                            DocumentClientMessage.ConflictWithExistingDocument.ToString());

                    case HttpStatusCode.RequestEntityTooLarge:
                        return BadRequest(logger, correlationId, EventType.CreateDocument,
                            DocumentClientMessage.RequestEntityTooLarge.ToString());
                }

                // HttpStatusCode does not have a TooManyRequest, so check with if statement
                if (ex.StatusCode != null && (int) ex.StatusCode == 429)
                {
                    return BadRequest(logger, correlationId, EventType.CreateDocument,
                        DocumentClientMessage.TooManyRequests.ToString());
                }
                return BadRequest(logger, correlationId, EventType.CreateDocument, null, ex);

            }
            catch (Exception ex)
            {
                return BadRequest(logger, correlationId, EventType.CreateDocument, null, ex);
            }
        }

        [FunctionName("ChangeFeedProcessor")]
        public static async Task Run(
            [CosmosDBTrigger(Db, Collection, ConnectionStringSetting = DbConnectionStringSetting, LeaseCollectionName = LeasesCollection)]IReadOnlyList<Event> events,
            [ServiceBus(Topic, Connection = ServiceBusSendConnectionStringSetting)]IAsyncCollector<Message> messageAsyncCollector,
            ILogger logger)
        {
            foreach (var eventModel in events)
            {
                PublishEvent(logger, messageAsyncCollector, eventModel);
            }
        }

        private static void PublishEvent(ILogger logger, IAsyncCollector<Message> messageAsyncCollector, Event eventModel)
        {
            try
            {
                messageAsyncCollector.AddAsync(new Message(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(eventModel)))
                {
                    ContentType = eventModel.EventType,
                    CorrelationId = eventModel.CorrelationId,
                    PartitionKey = eventModel.AggregateType,
                    MessageId = eventModel.EventId
                });
                logger.LogInformation(EventId(EventType.ChangeFeed), Template, EventTrigger, eventModel.CorrelationId,
                    EntityType, eventModel.EventId, EventType.ChangeFeed.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(EventId(EventType.ChangeFeed), ex, Template, EventTrigger, eventModel.CorrelationId,
                    EntityType, eventModel.EventId, EventType.ChangeFeed.ToString());
            }
        }

        private static EventId EventId(EventType eventType) => new EventId((int)eventType + EventBase);

        private static BadRequestObjectResult BadRequest(ILogger logger, string correlationId, EventType eventType, string message, Exception ex = null)
        {
            if (ex == null)
            {
                logger.LogWarning(EventId(eventType), Template, EventTrigger, correlationId, EntityType, null, $"{eventType.ToString()} {message}");
            }
            else
            {
                logger.LogError(EventId(eventType), ex, Template, EventTrigger, correlationId, EntityType, null, $"{eventType.ToString()} {ex.Message}");
                message = "unknown error";
            }
            return new BadRequestObjectResult(new JObject()["error"] = message);
        }
    }
}
