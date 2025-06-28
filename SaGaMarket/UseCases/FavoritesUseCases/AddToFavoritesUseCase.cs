using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
using System.ComponentModel.DataAnnotations;

public class AddToFavoritesUseCase
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<AddToFavoritesUseCase> _logger;

    public AddToFavoritesUseCase(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        ILogger<AddToFavoritesUseCase> logger)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(AddToFavoritesRequest request, Guid userId)
    {
        try
        {
            var user = await _cartRepository.GetUserWithCart(userId);
            if (user == null)
                throw new ArgumentException("User  not found");

            // Проверяем товар
            var product = await _productRepository.Get(request.ProductId);
            if (product == null)
                throw new ArgumentException("Product not found");

            // Проверяем, что товар уже в избранном
            if (user.ProductFromFavoriteIds.Contains(request.ProductId))
            {
                _logger.LogWarning("Product {ProductId} already in favorites", request.ProductId);
                return true; // Уже в избранном
            }

            user.ProductFromFavoriteIds.Add(request.ProductId);

            // Сохраняем изменения
            return await _cartRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to favorites for user {User Id}", userId);
            throw;
        }
    }

    public class AddToFavoritesRequest
    {
        [Required]
        public Guid ProductId { get; set; }
    }
}
