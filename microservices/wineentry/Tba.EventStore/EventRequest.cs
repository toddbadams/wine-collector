using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tba.EventStore
{
    public class EventRequest
    {
        /// <summary>
        /// The identifier of the aggregate (entity) that will have the event applied
        /// If null then create a new aggregate.
        /// </summary>
        [JsonProperty("aggregateId")]
        public string AggregateId { get; }

        /// <summary>
        /// The type of aggregate (entity), such a person, order, wine, etc.
        /// </summary>
        [JsonProperty("aggregateType")]
        [JsonRequired]
        public string AggregateType { get; }

        /// <summary>
        /// The type of event being applied to the aggregate, such as "BottlesPerCaseSet", etc.
        /// These event types must be validated in a higher layer.
        /// </summary>
        [JsonProperty("eventType")]
        [JsonRequired]
        public string EventType { get; }

        /// <summary>
        /// The version of type of event being applied to the aggregate.
        /// this allows versioned event handlers downstream
        ///     https://pdfs.semanticscholar.org/3488/7a031598b60c34ddf49033e1b5e13b9a7044.pdf
        /// </summary>
        [JsonProperty("eventTypeVersion")]
        [JsonRequired]
        public short EventTypeVersion { get; }

        /// <summary>
        /// A dynamic value or payload for the event being applied.
        /// This will be the value applied to the aggregate in the event handler downstream.
        /// </summary>
        [JsonProperty("value")]
        [JsonRequired]
        public JObject Value { get; }

        [JsonConstructor]
        public EventRequest(string aggregateId, string aggregateType, string eventType, short eventTypeVersion, JObject value)
        {
            AggregateId = string.IsNullOrWhiteSpace(aggregateId)
                ? aggregateId
                : throw new ArgumentNullException(nameof(aggregateId));
            AggregateType = !string.IsNullOrWhiteSpace(aggregateType)
                ? aggregateType
                : throw new ArgumentNullException(nameof(aggregateType));
            EventType = !string.IsNullOrWhiteSpace(eventType)
                ? eventType
                : throw new ArgumentNullException(nameof(eventType));
            EventTypeVersion = eventTypeVersion>= 0
                ? eventTypeVersion
                : throw new ArgumentOutOfRangeException(nameof(eventTypeVersion));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}