using Newtonsoft.Json;

namespace Tba.WineEntry.Upsert.Presentation.ApiModels
{
    public class UpsertResponse
    {
        private readonly string _id;
        private readonly int _version;

        public UpsertResponse(string id, int version)
        {
            _id = id;
            _version = version;
        }

        [JsonProperty("id")]
        public string Id { get;  }

        [JsonProperty("version")]
        public int Version { get;  }
    }
}
