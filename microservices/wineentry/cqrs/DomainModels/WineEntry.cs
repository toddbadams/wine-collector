using System;

namespace cqrs.DomainModels
{
    public class WineEntry
    {
        public int Quantity { get; set; }
        public BottleSize BottleSize { get; set; }
        public int BottlesPerCase { get; set; }
        public DateTimeOffset AcquiredOn { get; set; }
        public decimal CostPerBottle { get; set; }
        public Cellar Cellar { get; set; }
        public Wine Wine { get; set; }
    }
}