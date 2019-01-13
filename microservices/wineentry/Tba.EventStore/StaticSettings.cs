using Microsoft.Extensions.Logging;

namespace Tba.EventStore
{
    public static class StaticSettings
    {
        public const string Template = "WJ {Trigger}, {CorrelationId}, {EntityType}, {EntityId}, {Description}";
        public const string Db = "event-store";
        public const string Collection = "events";
        public const string LeasesCollection = "event-leases";
        public const string DbConnectionStringSetting = "CosmosConnectionString";
        public const int EventBase = 21000;


        public static EventId EventId(int methodId, int processStepId) => new EventId(methodId + processStepId + EventBase);
    }
}