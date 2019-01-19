using System;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tba.EventStore
{
    public class Event
    {
        /// <summary>
        /// The identifier of the aggregate (entity) that will have the event applied
        /// </summary>
        [JsonProperty("aggregateId")]
        public string AggregateId { get; }

        /// <summary>
        /// The type of aggregate (entity), such a person, order, wine, etc.
        /// </summary>
        [JsonProperty("aggregateType")]
        public string AggregateType { get; }

        /// <summary>
        /// A unique identifier for the event
        /// </summary>
        [JsonProperty("id")]
        public string EventId => $"{AggregateId}-{Sequence}";

        /// <summary>
        /// The type of event being applied to the aggregate, such as "BottlesPerCaseSet", etc.
        /// These event types must be validated in a higher layer.
        /// </summary>
        [JsonProperty("eventType")]
        public string EventType { get; }

        /// <summary>
        /// The version of type of event being applied to the aggregate.
        /// this allows versioned event handlers downstream
        /// </summary>
        [JsonProperty("eventTypeVersion")]
        public short EventTypeVersion { get; }

        /// <summary>
        /// These IDs are unique 64-bit unsigned integers, which are based on time, instead of being sequential.
        /// The full ID is composed of a timestamp, a worker number, and a sequence number.
        ///     https://developer.twitter.com/en/docs/basics/twitter-ids.html
        /// </summary>
        [JsonProperty("sequence")]
        public long Sequence { get; }

        /// <summary>
        /// A dynamic value or payload for the event being applied.
        /// This will be the value applied to the aggregate in the event handler downstream.
        /// </summary>
        [JsonProperty("value")]
        public JObject Value { get; }

        /// <summary>
        /// A unique correlation identifier that triggered this event
        /// </summary>
        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonConstructor]
        public Event(EventRequest request)
        {
            AggregateId = request.AggregateId;
            AggregateType = request.AggregateType;
            EventType = request.EventType;
            EventTypeVersion = request.EventTypeVersion;
            Value = request.Value;
        }

        public Message ToMessage()
        {
            return new Message(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this)))
            {
                ContentType = EventType,
                CorrelationId = CorrelationId,
                PartitionKey = AggregateType,
                MessageId = EventId
            };
        }
    }
}
