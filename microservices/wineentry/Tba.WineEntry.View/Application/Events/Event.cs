using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tba.WineEntry.View.Application.Events
{
    public class Event
    {
        [JsonProperty("aggregateId")]
        public string AggregateId { get; set; }

        [JsonProperty("name")]
        public EventName EventName { get; set; }

        [JsonProperty("sequence")]
        public int Sequence { get; set; }

        [JsonProperty("value")]
        public JObject Value { get; set; }

        public bool IsCreate => Sequence == 0;
    }
}
