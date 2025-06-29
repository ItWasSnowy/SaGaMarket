using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Storage.Repositories;
using System.ComponentModel.DataAnnotations;

public class RemoveFromCartUseCase
{
    private readonly ICartRepository _cartRepository;
    private readonly ILogger<RemoveFromCartUseCase> _logger;

    public RemoveFromCartUseCase(
        ICartRepository cartRepository,
        ILogger<RemoveFromCartUseCase> logger)
    {
        _cartRepository = cartRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(RemoveFromCartRequest request, Guid userId)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            var user = await _cartRepository.GetUserWithCart(userId);
            if (user == null)
                throw new ArgumentException("User  not found");

            if (!user.ProductFromCartIds.Contains(request.VariantId))
            {
                _logger.LogWarning("Variant {VariantId} not found in cart for user {User Id}", request.VariantId, userId);
                return false;
            }

            user.ProductFromCartIds.Remove(request.VariantId);

            return await _cartRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from cart for user {User Id}", userId);
            throw;
        }
    }

    public class RemoveFromCartRequest
    {
        [Required]
        public Guid VariantId { get; set; }
    }
}
