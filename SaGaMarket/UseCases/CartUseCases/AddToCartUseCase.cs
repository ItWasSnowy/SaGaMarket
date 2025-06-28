using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
using System.ComponentModel.DataAnnotations;

public class AddToCartUseCase
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IVariantRepository _variantRepository;
    private readonly ILogger<AddToCartUseCase> _logger;

    public AddToCartUseCase(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IVariantRepository variantRepository,
        ILogger<AddToCartUseCase> logger)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _variantRepository = variantRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(AddToCartRequest request, Guid userId)
    {
        try
        {
            var user = await _cartRepository.GetUserWithCart(userId);
            if (user == null)
                throw new ArgumentException("User not found");

            // Проверяем, что пользователь не является ТОЛЬКО продавцом
            if (user.Role == Role.seller && !user.ProductFromCartIds.Any())
                throw new InvalidOperationException("Sellers must have customer functionality enabled to add items to cart");

            // 2. Проверяем товар и вариант
            var product = await _productRepository.Get(request.ProductId);
            if (product == null)
                throw new ArgumentException("Product not found");

            var variant = await _variantRepository.Get(request.VariantId);
            if (variant == null || variant.ProductId != request.ProductId)
                throw new ArgumentException("Variant not found or doesn't belong to product");

            // 3. Добавляем в корзину (упрощенная реализация)
            if (user.ProductFromCartIds.Contains(request.VariantId))
            {
                _logger.LogWarning("Variant {VariantId} already in cart", request.VariantId);
                return true; // Уже в корзине
            }

            user.ProductFromCartIds.Add(request.VariantId);

            // 4. Сохраняем изменения
            return await _cartRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to cart for user {UserId}", userId);
            throw;
        }
    }

    public class AddToCartRequest
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid VariantId { get; set; }
    }
}