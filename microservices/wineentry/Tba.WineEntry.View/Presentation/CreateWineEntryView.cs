using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tba.WineEntry.View.Application.Configuration;
using EventHandler = Tba.WineEntry.View.Application.Events.EventHandler;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace Tba.WineEntry.View.Presentation
{
    public static class CreateWineEntryView
    {
        [FunctionName("CreateWineEntryView")]
        public static async Task Run(
            [ServiceBusTrigger(Config.WineEntryCreateTopic.Name, Config.WineEntryCreateTopic.Subscriber,
                Connection = Config.WineEntryCreateTopic.ListenConnectionStringSetting)]
            string message,
            string correlationId,
            [CosmosDBTrigger(Config.Db, Config.WineEntryStore.Collection,
                ConnectionStringSetting = Config.DbConnectionStringSetting)]IDocumentClient client,
            ILogger log)
        {
            if (EventHandler.TryDeserializeAndValidate(message, log, correlationId, out var e))
            {
                await CreateProcessor.Run(e, log, correlationId, client);
            }

        }
    }
}
