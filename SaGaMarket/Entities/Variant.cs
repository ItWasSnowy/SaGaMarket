namespace SaGaMarket.Core.Entities;

public class Variant
{
    public Guid VariantId { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    //public List<PriceGraph> PriceHistory { get; set; }
    public List<PriceGraph> priceGraph { get; set; }



    // Navigation properties
    public Product Product { get; set; }
    public List<PriceGraph> PriceHistory { get; set; } = new();
}
public class PriceGraph()
{
    public decimal Price;
    public DateTime LastPriceСhange { get; set; }
}