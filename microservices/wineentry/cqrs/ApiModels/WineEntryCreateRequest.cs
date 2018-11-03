using System;

namespace cqrs.ApiModels
{
    public class WineEntryCreateRequest
    {
        public int Quantity { get; set; }
        public int BottleSize { get; set; }
        public int BottlesPerCase { get; set; }
        public DateTimeOffset AcquiredOn { get; set; }
        public decimal CostPerBottle { get; set; }
        public Guid CellarId { get; set; }
        public Guid WineId { get; set; }
    }
}
