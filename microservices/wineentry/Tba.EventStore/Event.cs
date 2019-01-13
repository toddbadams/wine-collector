using System;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tba.EventStore
{
    public class Event
    {
        private enum ExceptionMessage
        {
            AggregateIdNullOrEmpty,
            AggregateTypeNullOrEmpty,
            EventTypeNullOrEmpty,
            SequenceNegative,
            ValueNull
        }

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

        /// <summary>
        /// Unique sequence of the event as applied to a specific aggregate.
        /// There is no way to ensure it is actually sequential at this layer, this must be validated at a higher layer.
        /// </summary>
        [JsonProperty("sequence")]
        public int Sequence { get; }

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
        public Event(Guid aggregateId, string aggregateType, int sequence, string eventType, JObject value)
        {
            AggregateId = aggregateId != Guid.Empty
                ? aggregateId
                : throw new ArgumentException(ExceptionMessage.AggregateIdNullOrEmpty.ToString());
            AggregateType = !string.IsNullOrWhiteSpace(aggregateType)
                ? aggregateType
                : throw new ArgumentException(ExceptionMessage.AggregateTypeNullOrEmpty.ToString());
            EventType = !string.IsNullOrWhiteSpace(eventType)
                ? eventType
                : throw new ArgumentException(ExceptionMessage.EventTypeNullOrEmpty.ToString());
            Sequence = sequence >= 0
                ? sequence
                : throw new ArgumentException(ExceptionMessage.SequenceNegative.ToString());
            Value = value ?? throw new ArgumentNullException(ExceptionMessage.ValueNull.ToString());
        }

        public  Message ToMessage()
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
