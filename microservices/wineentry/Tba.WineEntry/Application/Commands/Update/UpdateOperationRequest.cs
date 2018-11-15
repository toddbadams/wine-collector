using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tba.WineEntry.Application.Commands.Update
{
    public class UpdateOperationRequest
    {
        [JsonProperty("op")]
        [Required]
        public string Operation { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("value")]
        [Required]
        public JObject Value { get; set; }
    }
}