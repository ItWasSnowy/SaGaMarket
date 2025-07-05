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

            var order = new Order { CustomerId = customerId, TotalPrice = totalPrice, /*DiscountAmount = request.DiscountAmount,*/
                FinalPrice = totalPrice /*- request.DiscountAmount*/, ShippingAddress = request.ShippingAddress.Trim(),
                BillingAddress = (request.BillingAddress ?? request.ShippingAddress).Trim(),
                PaymentMethod = request.PaymentMethod.Trim(), orderStatus = OrderStatus.Processing,
                Notes = request.Notes?.Trim(), OrderItems = orderItems };

            return await _orderRepository.Create(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer: {CustomerId}", customerId);
            throw;
        }
    }

    private void ValidateRequest(CreateOrderRequest request)
    {
        if (request.Items == null || !request.Items.Any())
            throw new ArgumentException("Order must contain at least one item");

        //if (request.DiscountAmount < 0)
        //    throw new ArgumentException("Discount cannot be negative");

        if (string.IsNullOrWhiteSpace(request.ShippingAddress))
            throw new ArgumentException("Shipping address is required");
    }

    private async Task<(List<OrderItem> Items, decimal TotalPrice)> ProcessOrderItems(List<OrderItemDto> items)
    {
        var orderItems = new List<OrderItem>();
        
        decimal totalPrice = 0;

        foreach (var item in items)
        {
            var product = await _productRepository.Get(item.ProductId)
                ?? throw new ArgumentException($"Product not found: {item.ProductId}");

            var variant = await _variantRepository.Get(item.VariantId)
                ?? throw new ArgumentException($"Variant not found: {item.VariantId}");

            if (item.Quantity <= 0)
                throw new ArgumentException($"Invalid quantity for product: {item.ProductId}");

            if (variant.Price <= 0)
                throw new ArgumentException($"Invalid price for product: {item.ProductId}");

            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                Quantity = item.Quantity,
                UnitPrice = variant.Price,
                Product = product,
                Variant = variant
            };

            orderItems.Add(orderItem);
            totalPrice += item.Quantity * variant.Price;
        }

        return (orderItems, totalPrice);
    }

    public class CreateOrderRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one order item is required")]
        public List<OrderItemDto> Items { get; set; } = new();

        //[Range(0, double.MaxValue, ErrorMessage = "Discount cannot be negative")]
        //public decimal DiscountAmount { get; set; } = 0;

        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(200)]
        public string? BillingAddress { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "CreditCard";

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class OrderItemDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid VariantId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        //[Range(0.01, double.MaxValue)]
        //public decimal UnitPrice { get; set; }
    }
}