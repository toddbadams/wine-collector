using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using Tba.WineEntry.View.Application.Configuration;
using Tba.WineEntry.View.Application.Events;

namespace Tba.WineEntry.View.Application.Processors
{
    public static class UpdateProcessor
    {
        public static async Task Run(Event e, ILogger log, string correlationId, IDocumentClient client)
        {
            Domain.WineEntry wineEntry = null;
            try
            {
                wineEntry = await client.ReadDocumentAsync<Domain.WineEntry>(Config.WineEntryStore.DocumentUri(e.AggregateId));
                wineEntry.ApplyEvent(e);
                await client.UpsertDocumentAsync(Config.WineEntryStore.DocumentUri(e.AggregateId), wineEntry);
                log.LogInformation(Config.Logging.GetEventId(Config.Logging.EventType.ProcessingSucceeded),
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(), correlationId, nameof(Domain.WineEntry), e.AggregateId,
                    $"updated document");
            }
            // If either documentsFeedOrDatabaseLink or document is not set.
            catch (ArgumentNullException ex)
            {
                log.LogError(Config.Logging.GetEventId(Config.Logging.EventType.ProcessingFailedInvalidData),
                    ex,
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(), correlationId, nameof(Domain.WineEntry), e?.AggregateId,
                    $"documentsFeedOrDatabaseLink or document is not set");
            }
            // Represents a consolidation of failures that occured during async processing.
            // Look within InnerExceptions to find the actual exception(s)
            catch (AggregateException ex)
            {
                log.LogError(Config.Logging.GetEventId(Config.Logging.EventType.ValidationFailed),
                    ex,
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(), correlationId, nameof(Domain.WineEntry), e?.AggregateId,
                    $"a consolidation of failures");
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
                var m = "unknown error";
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        m = "something was wrong with the document supplied";
                        break;

                    case HttpStatusCode.Forbidden:
                        m = "collection in to which you were trying to create the document is full";
                        break;

                    case HttpStatusCode.Conflict:
                        m = "document with an id matching the id field of document already existed";
                        break;

                    case HttpStatusCode.RequestEntityTooLarge:
                        m = "document exceeds the current max entity size";
                        break;

                    case HttpStatusCode.TooManyRequests:
                        m = "exceeded the number of request units per second";
                        break;
                }
                log.LogError(Config.Logging.GetEventId(Config.Logging.EventType.ValidationFailed),
                    ex,
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(), correlationId, nameof(Domain.WineEntry), e?.AggregateId,
                    m);
            }
            catch (Exception ex)
            {
                log.LogError(Config.Logging.GetEventId(Config.Logging.EventType.ValidationFailed),
                    ex,
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(), correlationId, nameof(Domain.WineEntry), e?.AggregateId,
                    $"unhandled exception");
            }
        }
    }
}