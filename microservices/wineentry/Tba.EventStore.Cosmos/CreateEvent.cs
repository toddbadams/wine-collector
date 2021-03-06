using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
        public const string EventTrigger = "Http";
        public const string EntityType = "ApplicationEvent";
        private const int MethodLevelEventId = 100;

        // retry policy settings
        private const int RetryCount = 3;
        private static TimeSpan RetryDuration(int attempt) => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt));

        private enum ProcessStep
        {
            Deserialize = 1,
            Validation = 2,
            SequenceValidation = 3,
            CreateDocument = 4
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
            // get the correlation identifier
            request.Headers.TryGetValues(CorrelationIdHeaderKey, out var headerValues);
            var correlationId = headerValues == null ? Guid.NewGuid().ToString() : headerValues.First();

            // ProcessStep.Deserialize - deserialize request content
            EventRequest eventRequest;
            string payload = null;
            var eventId = StaticSettings.EventId(MethodLevelEventId, (int)ProcessStep.Deserialize, ProcessStep.Deserialize.ToString());
            try
            {
                payload = await request.Content.ReadAsStringAsync();
                eventRequest = JsonConvert.DeserializeObject<EventRequest>(payload);
                logger.LogInformation(eventId, StaticSettings.Template, OperationName, correlationId);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(logger, eventId, correlationId, ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(logger, eventId, correlationId, ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest(logger, supportNotifier, eventId, correlationId, e, payload);
            }

            // ProcessStep.Validation - validate the event request
            eventId = StaticSettings.EventId(MethodLevelEventId, (int)ProcessStep.Validation, ProcessStep.Validation.ToString());
            try
            {
                Validator.ValidateObject(eventRequest, new ValidationContext(eventRequest));
                logger.LogInformation(eventId, StaticSettings.Template, OperationName, correlationId);
            }
            catch (ValidationException)
            {
                return BadRequest(logger, eventId, correlationId, "Invalid request content");
            }
            catch (Exception e)
            {
                return BadRequest(logger, supportNotifier, eventId, correlationId, e, payload);
            }


            var docUri = UriFactory.CreateDocumentCollectionUri(StaticSettings.Db, StaticSettings.Collection);

            // check for valid event sequence of the event in our event store
            // This may result in a stale sequence number, but we catch that in the "create document" step
            if (eventRequest.EventPurpose != EventPurpose.CreateAggregate)
            {
                eventId = StaticSettings.EventId(MethodLevelEventId, (int)ProcessStep.SequenceValidation);
                var sequenceValidated = true;
                try
                {
                    Policy.Handle<Exception>()
                        .WaitAndRetry(RetryCount, RetryDuration, (ex, c) =>
                                logger.LogWarning(eventId, StaticSettings.Template, EventTrigger, correlationId, EntityType, 
                                    eventRequest.EventId, $"{ProcessStep.SequenceValidation.ToString()} retryTime: {c}, {ex.Message}"))
                        .Execute(ct =>
                            {
                                sequenceValidated = TryExecuteSequenceValidation(logger, eventId, correlationId,
                                    documentClient, docUri, eventRequest);
                            }, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    return BadRequest(logger, supportNotifier, eventId, correlationId, ProcessStep.SequenceValidation, ex,
                        payload);
                }

                if (!sequenceValidated)
                {
                    return BadRequest(logger, eventId, correlationId, ProcessStep.SequenceValidation, payload);
                }
            }

            // Create document
            // todo: implement retry
            eventId = StaticSettings.EventId(MethodLevelEventId, (int)ProcessStep.CreateDocument);
            try
            {
                await documentClient.CreateDocumentAsync(docUri, eventRequest);
                logger.LogInformation(eventId, StaticSettings.Template, EventTrigger, correlationId,
                    EntityType, eventRequest.EventId, ProcessStep.CreateDocument.ToString());
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

        private static bool TryExecuteSequenceValidation(ILogger logger, EventId eventId, string correlationId,
            IDocumentClient documentClient, Uri docUri, Event eventModel)
        {
            try
            {
                var sequence = documentClient.CreateDocumentQuery<int>(docUri, MaxSequence(eventModel.AggregateId))
                    .AsEnumerable().FirstOrDefault();
                if (eventModel.EventPurpose == EventPurpose.UpdateAggregate)
                {
                    eventModel.Sequence = sequence >= 0
                        ? sequence
                        : throw new ArgumentOutOfRangeException(nameof(documentClient));
                }

                if (eventModel.EventPurpose == EventPurpose.UpdateAggregateWithStrictVersion
                    && eventModel.Sequence != sequence + 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(sequence));
                }

                return true;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                logger.LogWarning(eventId, StaticSettings.Template, EventTrigger, correlationId, EntityType,
                    eventModel.EventId, $"{ProcessStep.SequenceValidation.ToString()} {ex.Message}");
                return false;
            }
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
        /// Handled exception
        /// </summary>
        private static BadRequestObjectResult BadRequest(ILogger logger, EventId eventId, string correlationId, string message)
        {
            logger.LogWarning(eventId, StaticSettings.Template, OperationName, correlationId, message);
            return new BadRequestObjectResult(new JObject()["error"] = message);
        }


        /// <summary>
        /// Unhandled exception
        /// </summary>
        private static BadRequestObjectResult BadRequest(ILogger logger, ISupportNotifier supportNotifier,
            EventId eventId, string correlationId, Exception ex, string payload)
        {
            var message =
                $"Unhandled Exception. Operation: {OperationName}, Step: {eventId.Name}, Request body: {payload}, Message: {ex.Message}";
            // log exception
            logger.LogError(eventId, ex, StaticSettings.Template, OperationName, correlationId, message);
            // return bad request with unknown exception
            return new BadRequestObjectResult(new JObject()["error"] = "unknown error");
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
