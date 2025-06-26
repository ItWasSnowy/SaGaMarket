namespace SaGaMarket.Core.Entities;

public class Variant
{
    public Guid VariantId { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    // Navigation properties
    public Product Product { get; set; }
    public PriceGraph priceHistory { get; set; } = new();
}
public class PriceGraph()
{
    public decimal Price { get; set; }
    public DateTime LastPriceChange { get; set; }
    // Внешний ключ для Variant
    public Guid VariantId { get; set; }
    public Variant Variant { get; set; } // Навигационное свойство
}