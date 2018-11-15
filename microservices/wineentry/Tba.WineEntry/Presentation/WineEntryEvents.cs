using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Tba.WineEntry.Application.Configuration;

namespace Tba.WineEntry.Presentation
{
    public static class WineEntryEvents
    {
        [FunctionName("WineEntryEvents")]
        public static void Run(
            [CosmosDBTrigger(Config.Db, Config.Collection,
                ConnectionStringSetting = Config.DbConnectionString,
            LeaseCollectionName = "leases")]IReadOnlyList<Document> input, 
            
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
            }
        }
    }
}
