using System;
using Microsoft.Extensions.Logging;

namespace Tba.WineEntry.Application.Configuration
{
    internal static class Config
    {
        internal const string Db = "WineEntry";
        internal const string Collection = "Wines";
        internal const string DbConnectionStringSetting = "CosmosConnectionString";
        internal const string TopicSendConnectionStringSetting = "TopicSendConnectionString";
        internal const string Route = "wines";
        internal const string Topic = "wine-entry";

        internal static class Logging
        {
            internal const string Template = "WineJargon {Trigger}, {CorrelationId}, {EntityType}, {EntityId}, {Description}";
            internal enum Trigger
            {
                Http,
                Publisher,
                Subscriber,
                ChangeFeed
            }

            internal enum EventType
            {
                DeserializationSucceeded = 1000,
                DeserializationFailed = 1001,
                ValidationSucceeded = 1002,
                ValidationFailed = 1003,
                ProcessingSucceeded = 1004,
                ProcessingFailedInvalidData = 1005,
                ProcessingFailedUnhandledException = 1006
            }

            internal static EventId GetEventId(EventType eventType) => new EventId((int)eventType);
        }
    }
}
