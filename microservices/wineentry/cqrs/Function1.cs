using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace cqrs
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([CosmosDBTrigger(
            databaseName: "accounting",
            collectionName: "invoices",
            ConnectionStringSetting = "CosmosDbConnectionString",
            LeaseCollectionName = "leases")]IReadOnlyList<Document> documents, TraceWriter log)
        {
            if (documents != null && documents.Count > 0)
            {
                log.Verbose("Documents modified " + documents.Count);
                log.Verbose("First document Id " + documents[0].Id);
            }
        }
    }
}
