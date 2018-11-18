using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Tba.WineEntry.View.Application.Events;

namespace Tba.WineEntry.View.Domain
{
    public class WineEntry
    {
        [JsonConstructor]
        public WineEntry(string id, int quantity, string bottleSize, string bottlesPerCase, DateTimeOffset acquiredOn,
            decimal costPerBottle, KeyValuePair<string, string> cellar, KeyValuePair<string, string> wine)
        {
            Id = id;
            Quantity = quantity;
            BottleSize = bottleSize;
            BottlesPerCase = bottlesPerCase;
            AcquiredOn = acquiredOn;
            CostPerBottle = costPerBottle;
            Cellar = cellar;
            Wine = wine;
        }

        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("quantity")]
        public int Quantity { get;  }

        [JsonProperty("BottleSize")]
        public string BottleSize { get; }

        [JsonProperty("bottlesPerCase")]
        public string BottlesPerCase { get;  }

        [JsonProperty("acquiredOn")]
        public DateTimeOffset AcquiredOn { get;  }

        [JsonProperty("costPerBottle")]
        public decimal CostPerBottle { get;  }

        [JsonProperty("cellar")]
        public KeyValuePair<string, string> Cellar { get;  }

        [JsonProperty("wine")]
        public KeyValuePair<string, string> Wine { get;}

        public void ApplyEvent(Event e)
        {
            // todo
        }
    }
}
