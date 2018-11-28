using Microsoft.Extensions.Logging;

namespace Tba.WineEntry.Configuration
{
    /// <summary>
    /// Application level configuration. These values do not change across environments.
    /// </summary>
    public static class Config
    {
        public static class Cosmos
        {
            public const string Db = "wine-entry-upsert";
            public const string DbConnectionStringSetting = "CosmosConnectionString";
            public const int MaxRetryAttemptsOnThrottledRequests = 9;
            public const int MaxRetryWaitTimeInSeconds = 30;

            public static class Events
            {
                public const string Collection = "events";
            }

            public enum ClientMessage
            {
                Success,
                DocumentNull,
                Aggregate,
                InvalidDocument,
                CollectionForbidden,
                ConflictWithExistingDocument,
                RequestEntityTooLarge,
                TooManyRequest,
                Unknown
            }
        }

        public static class ServiceBus
        {
            public static class WineEntry
            {
                public const string Topic = "wine-entry";
                public const string SendConnectionStringSetting = "TopicSendConnectionString";
            }
        }

        public static class Http
        {
            public const string Route = "wines";

            public enum ValidationMessage
            {
                Success,
                Invalid,
                Null,
                Unhandled
            }
        }

        public static class Logging
        {
            public const string Template = "WineJargon {Trigger}, {CorrelationId}, {EntityType}, {EntityId}, {Description}";
            public enum Trigger
            {
                Http,
                Publisher,
                Subscriber,
                ChangeFeed
            }

            public enum EventType
            {
                ValidationSucceeded = 1002,
                ValidationFailed = 1003,
                ProcessingSucceeded = 1004,
                ProcessingFailedInvalidData = 1005,
                ProcessingFailedUnhandledException = 1006
            }

            public static EventId GetEventId(EventType eventType) => new EventId((int)eventType);
        }
    }
}
