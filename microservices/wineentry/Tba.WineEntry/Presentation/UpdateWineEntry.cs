using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Tba.WineEntry.Application.Commands;
using Tba.WineEntry.Application.Commands.Create;
using Tba.WineEntry.Application.Commands.Update;
using Tba.WineEntry.Application.Configuration;

namespace cqrs
{
    public static class UpdateWineEntry
    {
        [FunctionName("UpdateWineEntry")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = Config.Route)]HttpRequestMessage req,
            [CosmosDBTrigger(Config.Db, Config.Collection, 
                ConnectionStringSetting = Config.DbConnectionStringSetting)]IAsyncCollector<Command> commandsOut,

            ILogger log)
        {
            var correlationId = Guid.NewGuid().ToString();
            // Deserialize & Validate
            UpdateRequest request = null;
            try
            {
                request = await req.Content.ReadAsAsync<UpdateRequest>();
                log.LogInformation(Config.Logging.GetEventId(Config.Logging.EventType.ValidationSucceeded),
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(),
                    correlationId,
                    nameof(UpdateRequest),
                    null,
                    $"CreateWineEntryRequest {JsonConvert.SerializeObject(request)}");
            }
            catch (Exception ex)
            {
                log.LogError(Config.Logging.GetEventId(Config.Logging.EventType.ValidationSucceeded),
                    ex,
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(),
                    correlationId,
                    nameof(UpdateRequest),
                    null,
                    $"CreateWineEntryRequest {JsonConvert.SerializeObject(request)}");
            }

            // Convert to commands
            Command command = null;
            try
            {
                command = new Command(Guid.NewGuid(), 0, EventName.WineEntryCreated, new JObject(request));
                log.LogInformation(Config.Logging.GetEventId(Config.Logging.EventType.ProcessingSucceeded),
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(),
                    correlationId,
                    nameof(Command),
                    command.AggregateId,
                    $"Command {JsonConvert.SerializeObject(command)}");

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
