using SaGaMarket.Core.Dtos;
using System.Diagnostics;

namespace SaGaMarket.Core.Entities;

public class Product
{
    public Guid ProductId { get; set; }
    public Guid SellerId { get; set; }
    public string Category { get; set; } = string.Empty;
    public double AverageRating { get; set; } = double.NaN;
    public List<Guid> ReviewIds { get; set; } = [];

    public User Seller { get; set; }
    public List<Variant> Variants { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
    public List<Tag> Tags { get; set; } = new();
    public List<Order> Orders { get; set; } = new();


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

