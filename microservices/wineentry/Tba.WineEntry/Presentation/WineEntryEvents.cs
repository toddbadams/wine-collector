using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tba.WineEntry.Application.Configuration;

namespace Tba.WineEntry.Presentation
{
    public static class WineEntryEvents
    {
        [FunctionName("WineEntryEvents")]
        public static async Task Run(
            [CosmosDBTrigger(Config.Db, Config.Collection,
                ConnectionStringSetting = Config.DbConnectionStringSetting,
            LeaseCollectionName = "leases")]IReadOnlyList<Document> events,
            [ServiceBus(Config.Topic, Connection = Config.TopicSendConnectionStringSetting)]IAsyncCollector<string> messageAsyncCollector,
            ILogger log)
        {
            foreach (var e in events)
            {
                await messageAsyncCollector.AddAsync(JsonConvert.SerializeObject(e));
                log.LogInformation(Config.Logging.GetEventId(Config.Logging.EventType.ProcessingSucceeded),
                    Config.Logging.Template,
                    Config.Logging.Trigger.ChangeFeed.ToString(),
                    null,
                    nameof(Document),
                    e.Id,
                    "wine entry event published");
            }
        }
    }
}
