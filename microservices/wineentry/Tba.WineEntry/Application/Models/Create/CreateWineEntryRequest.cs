using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Tba.WineEntry.Domain.Enums;

namespace Tba.WineEntry.Application.Models.Create
{
    public class CreateWineEntryRequest 
    {
        [JsonProperty("quantity")]
        [Required, Range(1,int.MaxValue)]
        public int Quantity { get; set; }

        [JsonProperty("BottleSize")]
        [Required]
        public BottleSize BottleSize { get; set; }

        [JsonProperty("bottlesPerCase")]
        [Required]
        public BottlesPerCase BottlesPerCase { get; set; }

        [JsonProperty("acquiredOn")]
        public DateTimeOffset AcquiredOn { get; set; }

        [JsonProperty("costPerBottle")]
        public decimal CostPerBottle { get; set; }

        [JsonProperty("cellar")]
        [Required]
        public KeyValuePair<Guid, string> Cellar { get; set; }

        [JsonProperty("wine")]
        [Required]
        public KeyValuePair<Guid, string> Wine { get; set; }
    }
}
