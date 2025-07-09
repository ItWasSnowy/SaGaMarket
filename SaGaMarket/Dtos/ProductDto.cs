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
    public string ImageUrl { get; set; }
    public int MinPrice { get; set; }
    public int MaxPrice { get; set; }
    public int VariantCount { get; set; }

    public ProductDto() { }
    public ProductDto(Product product)
    {
        ProductId = product.ProductId;
        SellerId = product.SellerId;
        ProductName = product.Name; // Добавлено
        ProductCategory = product.Category;
        AverageRating = product.AverageRating;
        ReviewIds = product.ReviewIds;

        // Рассчитываем минимальную и максимальную цену из вариантов
        if (product.Variants != null && product.Variants.Any())
        {
            MinPrice = (int)product.Variants.Min(v => v.Price);
            MaxPrice = (int)product.Variants.Max(v => v.Price);
            VariantCount = product.Variants.Count;
        }
        else
        {
            MinPrice = 0;
            MaxPrice = 0;
            VariantCount = 0;
        }
    }
}
