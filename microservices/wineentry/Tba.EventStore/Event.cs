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
        public Guid AggregateId { get; }

        /// <summary>
        /// The type of aggregate (entity), such a person, order, wine, etc.
        /// </summary>
        [JsonProperty("aggregateType")]
        public string AggregateType { get; }

        /// <summary>
        /// A unique identifier for the event, which is the unique key for the document.
        /// Adding sequence checks for unique key violations, every event on an aggregate must have a unique sequence.
        /// We cannot load this from the event store as the read may go stale before writing.
        /// </summary>
        [JsonProperty("id")]
        public string EventId => $"{AggregateId}-{Sequence}";

        /// <summary>
        /// The type of event being applied to the aggregate, such as "BottlesPerCaseSet", etc.
        /// These event types must be validated in a higher layer.
        /// </summary>
        [JsonProperty("eventType")]
        public string EventType { get; }

        [JsonProperty("eventPurpose")]
        public EventPurpose EventPurpose { get; }

        /// <summary>
        /// Unique sequence of the event as applied to a specific aggregate.
        /// There is no way to ensure it is actually sequential at this layer, this must be validated at a higher layer.
        /// </summary>
        [JsonProperty("sequence")]
        public int? Sequence { get; set; }

        /// <summary>
        /// A dynamic value or payload for the event being applied.
        /// This will be the value applied to the aggregate in the event handler which is done downstream.
        /// </summary>
        [JsonProperty("value")]
        public JObject Value { get; }

        /// <summary>
        /// A unique correlation identifier that triggered this event
        /// </summary>
        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonConstructor]
        public Event(Guid aggregateId, string aggregateType, int sequence, string eventType, string eventPurpose, JObject value)
        {
            AggregateId = aggregateId != Guid.Empty
                ? aggregateId
                : throw new ArgumentNullException(nameof(aggregateId));
            AggregateType = !string.IsNullOrWhiteSpace(aggregateType)
                ? aggregateType
                : throw new ArgumentNullException(nameof(aggregateType));
            EventType = !string.IsNullOrWhiteSpace(eventType)
                ? eventType
                : throw new ArgumentNullException(nameof(eventType));
            EventPurpose = (EventPurpose)Enum.Parse(typeof(EventPurpose), eventPurpose);
            switch (EventPurpose)
            {
                case EventPurpose.CreateAggregate:
                    Sequence = 0;
                    break;
                case EventPurpose.UpdateAggregate:
                    Sequence = null;
                    break;
                case EventPurpose.UpdateAggregateWithStrictVersion:
                    Sequence = sequence >= 1
                            ? sequence
                            : throw new ArgumentOutOfRangeException(nameof(sequence));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sequence));
            }
            Value = value ?? throw new ArgumentNullException(nameof(value));
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
