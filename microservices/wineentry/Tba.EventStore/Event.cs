using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tba.WineEntry
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

        [JsonProperty("aggregateId")]
        public Guid AggregateId { get; }

        [JsonProperty("aggregateType")]
        public string AggregateType { get; }

        [JsonProperty("id")]
        public string EventId => $"{AggregateId}-{Sequence}";

        [JsonProperty("eventType")]
        public string EventType { get; }

        [JsonProperty("sequence")]
        public int Sequence { get; }

        [JsonProperty("value")]
        public JObject Value { get; }

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
    }
}
