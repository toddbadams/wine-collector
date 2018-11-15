using AutoMapper;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Tba.WineEntry.Application.Commands;
using Tba.WineEntry.Application.Models.Create;
using Tba.WineEntry.Presentation.Configuration;

namespace cqrs
{
    public static class CreateWineEntry
    {
        [FunctionName("CreateWineEntry")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req,
            [CosmosDBTrigger(Config.WineEntry.Db, Config.WineEntry.Collection, 
                ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<Command> commandsOut,

            ILogger log)
        {
            var correlationId = Guid.NewGuid().ToString();
            // Deserialize & Validate
            CreateWineEntryRequest createRequest = null;
            try
            {
                createRequest = await req.Content.ReadAsAsync<CreateWineEntryRequest>();
                log.LogInformation(Config.Logging.GetEventId(Config.Logging.EventType.ValidationSucceeded),
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(),
                    correlationId,
                    nameof(CreateWineEntryRequest),
                    null,
                    $"CreateWineEntryRequest {JsonConvert.SerializeObject(createRequest)}");
            }
            catch (Exception ex)
            {
                log.LogError(Config.Logging.GetEventId(Config.Logging.EventType.ValidationSucceeded),
                    ex,
                    Config.Logging.Template,
                    Config.Logging.Trigger.Http.ToString(),
                    correlationId,
                    nameof(CreateWineEntryRequest),
                    null,
                    $"CreateWineEntryRequest {JsonConvert.SerializeObject(createRequest)}");
            }

            // Convert to commands
            Command command = null;
            try
            {
                command = new Command(Guid.NewGuid(), 0, CommandName.WineEntryCreated, new JObject(createRequest));
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
