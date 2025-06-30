using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GetProductWithPagination
{
    private readonly IProductRepository _productRepository;

    public GetProductWithPagination(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<(IEnumerable<ProductRequest> Products, int TotalCount)> GetProductsWithPaginationAsync(
        int page,
        int pageSize)
    {
        var (products, totalCount) = await _productRepository.GetProductsWithPaginationAsync(page, pageSize);

        var productDtos = products.Select(p => new ProductRequest
        {
            ProductId = p.ProductId,
            ProductName = p.Name,  // Используем p.Name вместо p.ProductName
            ProductCategory = p.Category,
            SellerId = p.SellerId,
            AverageRating = p.AverageRating,
            ReviewIds = p.ReviewIds,
            MinPrice = p.Variants.Any() ? (int)p.Variants.Min(v => v.Price) : 0,
            MaxPrice = p.Variants.Any() ? (int)p.Variants.Max(v => v.Price) : 0,
            VariantCount = p.Variants.Count
        }).ToList();

        return (productDtos, totalCount);
    }
}

public class ProductRequest 
{
    public Guid ProductId { get; set; }
    public Guid SellerId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCategory { get; set; } = string.Empty;
    public double AverageRating { get; set; } = double.NaN;
    public List<Guid> ReviewIds { get; set; } = [];
    public int MinPrice { get; set; }
    public int MaxPrice { get; set; }
    public int VariantCount { get; set; }
}