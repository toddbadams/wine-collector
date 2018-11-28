using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Tba.WineEntry.Configuration;

namespace Tba.WineEntry
{
    public class CosmosDbService
    {
        private static readonly IDocumentClient Client;

        static CosmosDbService()
        {
            var connectionString = Environment.GetEnvironmentVariable(Config.Cosmos.DbConnectionStringSetting);
            var connection = new CosmosDbConnectionString(connectionString);
            var connectionPolicy = new ConnectionPolicy();
            connectionPolicy.RetryOptions = new RetryOptions
            {
                MaxRetryAttemptsOnThrottledRequests = Config.Cosmos.MaxRetryAttemptsOnThrottledRequests,
                MaxRetryWaitTimeInSeconds = Config.Cosmos.MaxRetryWaitTimeInSeconds
            };
            Client = new DocumentClient(connection.ServiceEndpoint, connection.AuthKey, connectionPolicy);
        }

        public async Task<Config.Cosmos.ClientMessage> UpsertDocumentAsync(Uri documentCollectionUri, Document document)
        {
            try
            {
                ResourceResponse<Document> response = await Client.UpsertDocumentAsync(documentCollectionUri, document);
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

                        //case HttpStatusCode.TooManyRequests:
                        //    return Config.Cosmos.ClientMessage.TooManyRequest;
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
