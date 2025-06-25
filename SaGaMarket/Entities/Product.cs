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

    //Тут должны быть связи для EntityFramework



    public Product(){}
    public Product(ProductDto productDto)
    {
        ProductId = productDto.ProductId;
        SellerId = productDto.SellerId;
        Name = productDto.ProductName;
        Description = productDto.ProductDescription;
        Price = productDto.Price;
        PriceHistory = productDto.PriceHistory;
        Category = productDto.ProductCategory;
        AverageRating = productDto.AverageRating;
        ReviewIds = productDto.ReviewIds;
    }
}

