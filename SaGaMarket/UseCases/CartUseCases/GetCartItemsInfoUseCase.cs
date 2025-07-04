using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaGaMarket.Core.UseCases;

public class GetCartItemsInfoUseCase
{
    private readonly IVariantRepository _variantRepository;
    private readonly ILogger<GetCartItemsInfoUseCase> _logger;

    public GetCartItemsInfoUseCase(
        IVariantRepository variantRepository,
        ILogger<GetCartItemsInfoUseCase> logger)
    {
        _variantRepository = variantRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CartVariantInfoDto>> Execute(IEnumerable<Guid> variantIds)
    {
        _logger.LogInformation("Getting cart items info for {Count} variants", variantIds.Count());

        if (!variantIds.Any())
            return Enumerable.Empty<CartVariantInfoDto>();

        var variants = await _variantRepository.GetVariantsWithDetailsAsync(variantIds);

        return variants.Select(v => new CartVariantInfoDto
        {
            VariantId = v.VariantId,
            ProductId = v.ProductId,
            ProductName = v.Product?.Name ?? "Unknown Product",
            VariantName = v.Name,
            Description = v.Description,
            Price = v.Price,
            AvailableCount = v.Count,
            ImageUrl = v.ImageUrl ?? "/images/default-variant.png",
            ProductCategory = v.Product?.Category ?? "Uncategorized",
            ProductRating = v.Product?.AverageRating ?? 0,
            SellerName = "Unknown Seller"
        });
    }
}

public class CartVariantInfoDto
{
    public Guid VariantId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string VariantName { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int AvailableCount { get; set; }
    public string ImageUrl { get; set; }
    public string ProductCategory { get; set; }
    public double ProductRating { get; set; }
    public string SellerName { get; set; }
}