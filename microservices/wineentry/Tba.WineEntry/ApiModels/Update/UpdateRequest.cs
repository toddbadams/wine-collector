using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Newtonsoft.Json;

namespace Tba.WineEntry.ApiModels.Update
{
    public class UpdateRequest
    {
        [JsonProperty("token")]
        [Required]
        public string Token { get; set; }

        [JsonProperty("ops")]
        [Required, MinLength(1)]
        public IEnumerable<UpdateOperationRequest> Operations { get; set; }

        [JsonIgnore]
        public int Version { get; set; }

        [JsonIgnore]
        public Guid WineEntryId { get; set; }
    }
}
