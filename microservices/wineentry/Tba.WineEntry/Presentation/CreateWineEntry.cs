using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Tba.WineEntry.Application.Commands;
using Tba.WineEntry.Application.Commands.Create;
using Tba.WineEntry.Application.Configuration;

namespace cqrs
{
    public static class CreateWineEntry
    {
        [FunctionName("CreateWineEntry")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Config.Route)]HttpRequestMessage req,
            [CosmosDBTrigger(Config.Db, Config.Collection, 
                ConnectionStringSetting = Config.DbConnectionStringSetting)]IAsyncCollector<Command> commandsOut,

            ILogger log)
        {
            var correlationId = Guid.NewGuid().ToString();
            // Deserialize & Validate
            CreateWineEntryRequest createRequest = null;
            try
            {
                createRequest = await req.Content.ReadAsAsync<CreateWineEntryRequest>();
                Validator.ValidateObject(createRequest,new ValidationContext(createRequest));
                log.LogInformation(Config.Logging.GetEventId(Config.Logging.EventType.ValidationSucceeded),
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(),
                    correlationId,
                    nameof(CreateWineEntryRequest),
                    null,
                    $"Valid request");
            }
            catch (ArgumentNullException ex)
            {
                log.LogError(Config.Logging.GetEventId(Config.Logging.EventType.ValidationFailed),
                    ex,
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(),
                    correlationId,
                    nameof(CreateWineEntryRequest),
                    null,
                    $"request is null");
            }
            catch (ValidationException ex)
            {
                log.LogError(Config.Logging.GetEventId(Config.Logging.EventType.ValidationFailed),
                    ex,
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(),
                    correlationId,
                    nameof(CreateWineEntryRequest),
                    null,
                    $"Validation failed {ex.Message}");
            }
            catch (Exception ex)
            {
                log.LogError(Config.Logging.GetEventId(Config.Logging.EventType.ValidationFailed),
                    ex,
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(),
                    correlationId,
                    nameof(CreateWineEntryRequest),
                    null,
                    $"unhandled exception");
            }

            // Convert to commands
            Command command = null;
            try
            {
                command = new Command(Guid.NewGuid(), 0, EventName.WineEntryCreated, JObject.FromObject(createRequest));
                log.LogInformation(Config.Logging.GetEventId(Config.Logging.EventType.ProcessingSucceeded),
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(),
                    correlationId,
                    nameof(Command),
                    command.AggregateId,
                    $"Command created");
                // Write commands to data store
                await commandsOut.AddAsync(command);
            }
            catch (Exception ex)
            {
                log.LogError(Config.Logging.GetEventId(Config.Logging.EventType.ProcessingFailedUnhandledException),
                    ex,
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(),
                    correlationId,
                    nameof(Command),
                    command?.AggregateId,
                    ex.Message);
            }

            // return
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}
