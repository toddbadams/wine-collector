using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cqrs.Commands
{
    public class SimpleCommandValue<T> : JObject
    {
        public SimpleCommandValue(T value)
        {
            Value = value != null ? value : throw new ArgumentNullException(nameof(value));
        }

        [JsonProperty("value")]
        public T Value { get; }
    }
}