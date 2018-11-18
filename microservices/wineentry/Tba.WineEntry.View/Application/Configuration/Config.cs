using System;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;

namespace Tba.WineEntry.View.Application.Configuration
{
    internal static class Config
    {
        internal const string Db = "wine-entry-view";
        internal const string DbConnectionStringSetting = "CosmosConnectionString";
        internal const string Route = "wines";

        internal static class WineEntryStore
        {
            internal const string Collection = "wine-entries";
            internal static Uri Uri => UriFactory.CreateDocumentCollectionUri(Db, Collection);
            internal static Uri DocumentUri(string id) => UriFactory.CreateDocumentUri(Db, Collection, id);
        }

        internal static class WineEntryCreateTopic
        {
            internal const string Name = "wine-entry-create";
            internal const string Subscriber = "wine-entry-view";
            internal const string ListenConnectionStringSetting = "CreateTopicSendConnectionString";
        }

        internal static class WineEntryUpdateTopic
        {
            internal const string Name = "wine-entry-update";
            internal const string Subscriber = "wine-entry-view";
            internal const string ListenConnectionStringSetting = "UpdateTopicSendConnectionString";
        }

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
