using cqrs;

namespace Tba.WineEntry.Domain.Entities
{
    public class Cellar
    {
        public CellarType Type { get; set; }
        public string Name { get; set; }
        public string AccountReference { get; set; }
        public string Address { get; set; }
    }
}