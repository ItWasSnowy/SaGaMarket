using System.Text.Json.Serialization;

namespace SaGaMarket.Core.Entities;

public class Variant
{
    public Guid VariantId { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Count { get; set; }

    [JsonIgnore]
    public Product Product { get; set; } = null!;
    public PriceGraph PriceHistory { get; set; } = new();
}
public class PriceGraph
{
    public decimal Price { get; set; }
    public DateTime LastPriceChange { get; set; } = DateTime.UtcNow;
    public Guid VariantId { get; set; }
    [JsonIgnore]
    public Variant Variant { get; set; } = null!;
}