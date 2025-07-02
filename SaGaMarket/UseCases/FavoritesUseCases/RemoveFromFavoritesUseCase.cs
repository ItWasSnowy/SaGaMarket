using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Storage.Repositories;
using System.ComponentModel.DataAnnotations;

public class RemoveFromFavoritesUseCase
{
    private readonly ICartRepository _cartRepository;
    private readonly ILogger<RemoveFromFavoritesUseCase> _logger;

    public RemoveFromFavoritesUseCase(
        ICartRepository cartRepository,
        ILogger<RemoveFromFavoritesUseCase> logger)
    {
        _cartRepository = cartRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(RemoveFromFavoritesRequest request, Guid userId)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            var user = await _cartRepository.GetUserWithCart(userId);
            if (user == null)
                throw new ArgumentException("User  not found");

            if (!user.ProductFromFavoriteIds.Contains(request.ProductId))
            {
                _logger.LogWarning("Product {ProductId} not found in favorites for user {User Id}", request.ProductId, userId);
                return false;
            }

            user.ProductFromFavoriteIds.Remove(request.ProductId);

            return await _cartRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from favorites for user {User Id}", userId);
            throw;
        }
    }

    public class RemoveFromFavoritesRequest
    {
        [Required]
        public Guid ProductId { get; set; }
    }
}
