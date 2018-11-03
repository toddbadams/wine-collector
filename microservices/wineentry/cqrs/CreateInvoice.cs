using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;
using AutoMapper;
using cqrs.ApiModels;
using cqrs.Commands;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace cqrs
{
    public static class CreateInvoice
    {
        private const string Db = "Accounting";
        private const string Collection = "Accounting";

        static CreateInvoice()
        {
            Mapper.Initialize(_ => _.CreateMap<WineEntryCreateRequest, CommandCollection>()
                .ConvertUsing(new WineEntryCreateRequestConverter()));
        }

        [FunctionName("CreateInvoice")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req,
            [CosmosDBTrigger(Db,Collection, ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<Command> commandsOut,

            ILoggerProvider log)
        {
            // Validate
            var createRequest = await req.Content.ReadAsAsync<WineEntryCreateRequest>();

            // Convert to commands
            var commands = Mapper.Map<CommandCollection>(createRequest);

            // Write commands to data store
            await Task.WhenAll(commands.Select(_ => commandsOut.AddAsync(_)).ToArray());

            // return
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}
