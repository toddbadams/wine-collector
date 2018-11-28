using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Tba.WineEntry.Configuration;
using Tba.WineEntry.View.Application.Processors;
using EventHandler = Tba.WineEntry.View.Application.Events.EventHandler;

namespace Tba.WineEntry.View.Presentation
{
    public static class CreateWineEntryView
    {
        [FunctionName("CreateWineEntryView")]
        public static async Task Run(
            [ServiceBusTrigger(Config.ServiceBus.WineEntryCreate.Topic, Config.ServiceBus.WineEntryCreate.Subscriber,
                Connection = Config.ServiceBus.WineEntryCreate.ListenConnectionStringSetting)]
            string message,
            string correlationId,
            [CosmosDBTrigger(Config.Cosmos.Db, Config.Cosmos.WineEntry.Collection,
                ConnectionStringSetting = Config.Cosmos.DbConnectionStringSetting)]IDocumentClient client,
            ILogger log)
        {
            if (EventHandler.TryDeserializeAndValidate(message, log, correlationId, out var e))
            {
                await CreateProcessor.Run(e, log, correlationId, client);
            }

        }
    }
}
