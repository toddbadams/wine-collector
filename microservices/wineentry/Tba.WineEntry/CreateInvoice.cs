using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using AutoMapper;
using cqrs.Commands;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Tba.WineEntry.ApiModels.Create;

namespace cqrs
{
    public static class CreateInvoice
    {
        private const string Db = "Accounting";
        private const string Collection = "Accounting";

        static CreateInvoice()
        {
            Mapper.Initialize(_ => _.CreateMap<CreateRequest, CommandCollection>()
                .ConvertUsing(new CreateRequestConverter()));
        }

        [FunctionName("CreateInvoice")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req,
            [CosmosDBTrigger(Db,Collection, ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<Command> commandsOut,

            ILoggerProvider log)
        {
            // Validate
            var createRequest = await req.Content.ReadAsAsync<CreateRequest>();

            // Convert to commands
            var commands = Mapper.Map<CommandCollection>(createRequest);

            // Write commands to data store
            await Task.WhenAll(commands.Select(_ => commandsOut.AddAsync(_)).ToArray());

            // return
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}
