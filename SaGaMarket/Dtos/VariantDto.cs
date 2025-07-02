using SaGaMarket.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaGaMarket.Core.Dtos
{
    public class VariantDto
    {
        public Guid VariantId { get; set; }
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Count { get; set; }
        public PriceGraph PriceHistory { get; set; } = new();
    }
    public class PriceGraphDto
    {
        public decimal Price { get; set; }
        public DateTime LastPriceChange { get; set; } = DateTime.UtcNow;

        public Guid VariantId { get; set; }
    }
}
