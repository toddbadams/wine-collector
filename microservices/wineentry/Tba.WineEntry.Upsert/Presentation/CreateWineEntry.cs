using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Tba.WineEntry.Upsert.Application.Commands;
using Tba.WineEntry.Upsert.Presentation.ApiModels;
using Tba.WineEntry.Configuration;

namespace Tba.WineEntry.Upsert.Presentation
{
    public static class CreateWineEntry
    {
        [FunctionName("CreateWineEntry")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Config.Http.Route)]HttpRequestMessage req,
            [CosmosDBTrigger(Config.Cosmos.Db, Config.Cosmos.Events.Collection,
                ConnectionStringSetting = Config.Cosmos.DbConnectionStringSetting)]IAsyncCollector<Command> commandsOut,

            ILogger log)
        {
            var correlationId = Guid.NewGuid().ToString();
            var validator = new CreateWineEntryValidator();
            // Deserialize & Validate
            if (!validator.TryDeserializeAndValidate(await req.Content.ReadAsStringAsync(), out var createRequest,
                out var message))
            {
                log.LogWarning(Config.Logging.GetEventId(Config.Logging.EventType.ValidationFailed),
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(),
                    correlationId,
                    nameof(CreateWineEntryRequest),
                    null,
                    message);
                return new BadRequestObjectResult(new JObject()["error"] = message);
            }
            log.LogInformation(Config.Logging.GetEventId(Config.Logging.EventType.ValidationSucceeded),
                Config.Logging.Template,
                Config.Logging.Trigger.Http.ToString(),
                correlationId,
                nameof(CreateWineEntryRequest),
                null,
                message);

            // Convert to commands
            var command = new Command(Guid.NewGuid(), 0, EventName.WineEntryCreated, JObject.FromObject(createRequest));
            var clientMessage = await AddAsync(command, commandsOut);
            if (clientMessage != Config.Cosmos.ClientMessage.Success)
            {
                log.LogError(Config.Logging.GetEventId(clientMessage == Config.Cosmos.ClientMessage.Unknown ?
                        Config.Logging.EventType.ProcessingFailedUnhandledException :
                            Config.Logging.EventType.ProcessingFailedInvalidData),
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(),
                    correlationId,
                    nameof(Command),
                    command.AggregateId,
                    clientMessage.ToString());
            }

            log.LogInformation(Config.Logging.GetEventId(Config.Logging.EventType.ProcessingSucceeded),
                Config.Logging.Template,
                Config.Logging.Trigger.Http.ToString(),
                correlationId,
                nameof(Command),
                command.AggregateId,
                clientMessage);

            return new OkObjectResult(new UpsertResponse(command.AggregateId.ToString(), command.Sequence));
        }

        private static async Task<Config.Cosmos.ClientMessage> AddAsync(Command command, IAsyncCollector<Command> commandsOut)
        {

            try
            {
                await commandsOut.AddAsync(command);
                return Config.Cosmos.ClientMessage.Success;
            }
            // If either documentsFeedOrDatabaseLink or document is not set.
            catch (ArgumentNullException)
            {
                return Config.Cosmos.ClientMessage.DocumentNull;
            }
            // Represents a consolidation of failures that occured during async processing.
            // Look within InnerExceptions to find the actual exception(s)
            catch (AggregateException)
            {
                return Config.Cosmos.ClientMessage.Aggregate;
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
                        return Config.Cosmos.ClientMessage.InvalidDocument;

                    case HttpStatusCode.Forbidden:
                        return Config.Cosmos.ClientMessage.CollectionForbidden;

                    case HttpStatusCode.Conflict:
                        return Config.Cosmos.ClientMessage.ConflictWithExistingDocument;

                    case HttpStatusCode.RequestEntityTooLarge:
                        return Config.Cosmos.ClientMessage.RequestEntityTooLarge;

                    case HttpStatusCode.TooManyRequests:
                        return Config.Cosmos.ClientMessage.TooManyRequest;
                }
                return Config.Cosmos.ClientMessage.Unknown;

            }
            catch (Exception)
            {
                return Config.Cosmos.ClientMessage.Unknown;
            }
        }
    }
}
