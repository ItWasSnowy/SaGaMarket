using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
using System.ComponentModel.DataAnnotations;

public class CreateOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IVariantRepository _variantRepository;
    private readonly ILogger<CreateOrderUseCase> _logger;

    public CreateOrderUseCase(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IVariantRepository variantRepository,
        ILogger<CreateOrderUseCase> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _variantRepository = variantRepository;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateOrderRequest request, Guid customerId)
    {
        try
        {
            ValidateRequest(request);

            var (orderItems, totalPrice) = await ProcessOrderItems(request.Items);

            var order = new Order
            {
                CustomerId = customerId,
                TotalPrice = totalPrice,
                FinalPrice = totalPrice,
                ShippingAddress = request.ShippingAddress.Trim(),
                BillingAddress = (request.BillingAddress ?? request.ShippingAddress).Trim(),
                PaymentMethod = request.PaymentMethod.Trim(),
                orderStatus = OrderStatus.Processing,
                Notes = request.Notes?.Trim(),
                OrderItems = orderItems
            };

            return await _orderRepository.Create(order);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ошибка валидации при создании заказа для пользователя {CustomerId}", customerId);
            throw; // Перебрасываем исключение для обработки в контроллере
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании заказа для пользователя: {CustomerId}", customerId);
            throw new Exception("Произошла ошибка при создании заказа. Пожалуйста, попробуйте позже.");
        }
    }

    private void ValidateRequest(CreateOrderRequest request)
    {
        if (request.Items == null || !request.Items.Any())
            throw new ArgumentException("Заказ должен содержать хотя бы один товар");

        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
            throw new ArgumentException("Адрес доставки обязателен для заполнения");

        if (string.IsNullOrWhiteSpace(request.PaymentMethod))
            throw new ArgumentException("Не указан способ оплаты");
    }

    private async Task<(List<OrderItem> Items, decimal TotalPrice)> ProcessOrderItems(List<OrderItemDto> items)
    {
        var orderItems = new List<OrderItem>();
        decimal totalPrice = 0;

        // Получаем все необходимые продукты и варианты одним запросом
        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var variantIds = items.Select(i => i.VariantId).Distinct().ToList();

        var products = await _productRepository.GetByIds(productIds);
        var variants = await _variantRepository.GetByIds(variantIds);

        foreach (var item in items)
        {
            var product = products.FirstOrDefault(p => p.ProductId == item.ProductId);
            if (product == null)
                throw new ArgumentException($"Товар с ID {item.ProductId} не найден");

            var variant = variants.FirstOrDefault(v => v.VariantId == item.VariantId);
            if (variant == null)
                throw new ArgumentException($"Вариант товара с ID {item.VariantId} не найден");

            if (item.Quantity <= 0)
                throw new ArgumentException($"Неверное количество товара {product.Name}");

            if (variant.Price <= 0)
                throw new ArgumentException($"Неверная цена для товара {product.Name}");

            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                Quantity = item.Quantity,
                UnitPrice = variant.Price,
                // Не присваиваем навигационные свойства Product и Variant,
                // чтобы избежать проблем с отслеживанием сущностей
                OrderStatus = OrderItemStatus.Reserved
            };

            orderItems.Add(orderItem);
            totalPrice += item.Quantity * variant.Price;
        }

        return (orderItems, totalPrice);
    }

    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "Список товаров обязателен")]
        [MinLength(1, ErrorMessage = "Заказ должен содержать хотя бы один товар")]
        public List<OrderItemDto> Items { get; set; } = new();

        [Required(ErrorMessage = "Адрес доставки обязателен")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Адрес доставки должен содержать от 5 до 200 символов")]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Адрес оплаты не должен превышать 200 символов")]
        public string? BillingAddress { get; set; }

        [Required(ErrorMessage = "Способ оплаты обязателен")]
        public string PaymentMethod { get; set; } = "CreditCard";

        [StringLength(500, ErrorMessage = "Примечание не должно превышать 500 символов")]
        public string? Notes { get; set; }
    }

    public class OrderItemDto
    {
        [Required(ErrorMessage = "ID товара обязателен")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "ID варианта товара обязателен")]
        public Guid VariantId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть не менее 1")]
        public int Quantity { get; set; }
    }
}