using SaGaMarket.Core.Dtos;
using System.Diagnostics;

namespace SaGaMarket.Core.Entities;

public class Product
{
    public Guid ProductId { get; set; }
    public Guid SellerId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double AverageRating { get; set; } = double.NaN;
    public List<Guid> ReviewIds { get; set; } = [];
    public User Seller { get; set; } = null!;
    public List<Variant> Variants { get; } = new();
    public List<Review> Reviews { get; } = new();
    public List<Tag> Tags { get; } = new();
    public List<Order> Orders { get; } = new();
    public Product(){}
    public Product(ProductDto productDto)
    {
        ProductId = productDto.ProductId;
        SellerId = productDto.SellerId;
        Category = productDto.ProductCategory;
        AverageRating = productDto.AverageRating;
        ReviewIds = productDto.ReviewIds;
    }
}

