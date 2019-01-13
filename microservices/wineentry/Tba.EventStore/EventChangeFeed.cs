using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Tba.EventStore
{
    public static class EventChangeFeed
    {
        private const string OperationName = "TBA-application-event-change-feed";
        private const int MethodEventBase = 100;
        private const string EventTrigger = "CosmosDb";
        private const string EntityType = "ApplicationEvent";
        private const string Topic = "";
        private const string ServiceBusSendConnectionStringSetting = "";

        private enum EventType
        {
            ChangeFeed = 1
        }

        private enum PublishMessage
        {
            PublishedApplicationEvent
        }

        [FunctionName(OperationName)]
        public static async Task Run(
            [CosmosDBTrigger(StaticSettings.Db, StaticSettings.Collection, ConnectionStringSetting = StaticSettings.DbConnectionStringSetting, 
                LeaseCollectionName = StaticSettings.LeasesCollection)]IReadOnlyList<Event> events,
            [ServiceBus(Topic, Connection = ServiceBusSendConnectionStringSetting)]IAsyncCollector<Message> messageAsyncCollector,
            ISupportNotifier supportNotifier,
            ILogger logger)
        {
            foreach (var eventModel in events)
            {
                PublishEvent(logger, supportNotifier, messageAsyncCollector, eventModel);
            }
        }


        private static void PublishEvent(ILogger logger, ISupportNotifier supportNotifier, IAsyncCollector<Message> messageAsyncCollector, Event eventModel)
        {
            var eventId = StaticSettings.EventId(MethodEventBase, (int)EventType.ChangeFeed);
            try
            {
                messageAsyncCollector.AddAsync(eventModel.ToMessage());
                logger.LogInformation(eventId, StaticSettings.Template, EventTrigger, eventModel.CorrelationId,
                    EntityType, eventModel.EventId, PublishMessage.PublishedApplicationEvent.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(eventId, ex, StaticSettings.Template, EventTrigger, eventModel.CorrelationId,
                    EntityType, eventModel.EventId, ex.Message);
                // notify support of unknown exception
                supportNotifier.Notify($"WJ Unhandled Exception. Operation: {OperationName}, body: {JsonConvert.SerializeObject(eventModel)}, Message: {ex.Message}");
            }
        }
    }
}