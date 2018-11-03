using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cqrs.Commands
{
    public class Command
    {
        [JsonProperty("aggregateId")]
        public Guid AggregateId { get; }

        [JsonProperty("id")]
        public string Id => $"{AggregateId}-{Sequence}";

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("sequence")]
        public int Sequence { get; }

        [JsonProperty("value")]
        public JObject Value { get; }

        [JsonConstructor]
        public Command(Guid aggregateId, int sequence, CommandName name, JObject value)
        {
            AggregateId = aggregateId != Guid.Empty
                ? aggregateId
                : throw new ArgumentException("aggregateId cannot be empty");
            Sequence = sequence >= 0
                ? sequence
                : throw new ArgumentException("sequence cannot be negative");
            Name = name.ToString();
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}
