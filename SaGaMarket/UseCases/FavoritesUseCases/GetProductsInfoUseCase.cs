using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaGaMarket.Core.UseCases
{
    public class GetProductsInfoUseCase
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<GetProductsInfoUseCase> _logger;

        public GetProductsInfoUseCase(
            IProductRepository productRepository,
            ILogger<GetProductsInfoUseCase> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<List<ProductInfoDto>> Execute(IEnumerable<Guid> productIds)
        {
            _logger.LogInformation("Getting info for {Count} products", productIds.Count());

            if (!productIds.Any())
                return new List<ProductInfoDto>();

            var products = await _productRepository.GetProductsWithDetailsAsync(productIds);

            return products.Select(p => new ProductInfoDto
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Category = p.Category,
                AverageRating = p.AverageRating,
                SellerName = /*p.Seller?.UserName ??*/ "Unknown Seller",
                ImageUrl = p.Variants.FirstOrDefault()?.ImageUrl ?? "/images/default-product.png"
            }).ToList();
        }
    }

    public class ProductInfoDto
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public double AverageRating { get; set; }
        public string SellerName { get; set; }
        public string ImageUrl { get; set; }
    }
}