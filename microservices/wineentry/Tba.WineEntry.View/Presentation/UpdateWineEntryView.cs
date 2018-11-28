using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Tba.WineEntry.View.Application.Configuration;
using Tba.WineEntry.View.Application.Processors;
using EventHandler = Tba.WineEntry.View.Application.Events.EventHandler;

namespace Tba.WineEntry.View.Presentation
{
    public static class UpdateWineEntryView
    {
        [FunctionName("UpdateWineEntryView")]
        public static async Task Run(
            [ServiceBusTrigger(Config.WineEntryUpdateTopic.Name, Config.WineEntryUpdateTopic.Subscriber,
                Connection = Config.WineEntryUpdateTopic.ListenConnectionStringSetting)]
            string message,
            string correlationId,
            [CosmosDBTrigger(Config.Db, Config.WineEntryStore.Collection,
                ConnectionStringSetting = Config.DbConnectionStringSetting)]IDocumentClient client,
            ILogger log)
        {
            if (EventHandler.TryDeserializeAndValidate(message, log, correlationId, out var e))
            {
                await UpdateProcessor.Run(e, log, correlationId, client);
            }
        }
    }
}
