using SaGaMarket.Core.Entities;
using System.Diagnostics;

namespace SaGaMarket.Core.Dtos;

public class ProductDto
{
    public Guid ProductId { get; set; }
    public Guid SellerId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<decimal> PriceHistory { get; set; }
    public string ProductCategory { get; set; } = string.Empty;
    public double AverageRating { get; set; } = double.NaN;
    public List<Guid> ReviewIds { get; set; } = [];

    public ProductDto() { }
    public ProductDto(Product product)
    {
        ProductId = product.ProductId;
        SellerId = product.SellerId;
        ProductName = product.Name;
        ProductDescription = product.Description;
        Price = product.Price;
        PriceHistory = product.PriceHistory;
        ProductCategory = product.Category;
        AverageRating = product.AverageRating;
        ReviewIds = product.ReviewIds;
    }
}
