using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Services;
using SaGaMarket.Core.Storage.Repositories;


namespace SaGaMarket.Core.UseCases.OrderUseCases
{
    public class AddVariantToOrderFromCartUseCase
    {

        private readonly IOrderRepository _orderRepository;
        private readonly IVariantRepository _variantRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRoleService _userRoleService;
        private readonly ILogger<AddVariantToOrderFromCartUseCase> _logger;

        public AddVariantToOrderFromCartUseCase(

            IOrderRepository orderRepository,
            IVariantRepository variantRepository,
            IProductRepository productRepository,
            IUserRoleService userRoleService,
            ILogger<AddVariantToOrderFromCartUseCase> logger)
        {
            _orderRepository = orderRepository;
            _variantRepository = variantRepository;
            _productRepository = productRepository;
            _userRoleService = userRoleService;
            _logger = logger;
        }

        public class AddVariantToOrderRequest
        {
            public Guid ProductId { get; set; }
            public Guid VariantId { get; set; }
            public int Quantity { get; set; }
        }

        public async Task<Guid> Handle(AddVariantToOrderRequest request, Guid orderId, Guid userId)
        {


            try
            {
                // 1. Проверка существования варианта
                var variant = await _variantRepository.Get(request.VariantId);
                if (variant == null)
                {
                    throw new ArgumentException("Вариант товара не найден");
                }

                // 2. Проверка существования товара
                var product = await _productRepository.Get(request.ProductId);
                if (product == null)
                {
                    throw new ArgumentException("Товар не найден");
                }

                // 3. Проверка доступного количества
                if (variant.Count < request.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Недостаточно товара в наличии. Доступно: {variant.Count}");
                }

                // 4. Проверка прав доступа
                if (product.SellerId != userId && !await _userRoleService.IsAdmin(userId))
                {
                    throw new UnauthorizedAccessException(
                        "У вас нет прав на добавление этого товара в заказ");
                }

                // 5. Проверка принадлежности заказа
                var order = await _orderRepository.Get(orderId);
                if (order == null)
                {
                    throw new ArgumentException("Заказ не найден");
                }

                if (order.CustomerId != userId && !await _userRoleService.IsAdmin(userId))
                {
                    throw new UnauthorizedAccessException(
                        "У вас нет прав на изменение этого заказа");
                }

                // 6. Создание позиции заказа
                var orderItem = new OrderItem
                {
                    OrderId = orderId,
                    ProductId = request.ProductId,
                    VariantId = request.VariantId,
                    Quantity = request.Quantity,
                    UnitPrice = variant.Price,
                    OrderStatus = OrderItemStatus.Reserved
                };

                // 7. Обновление количества товара
                variant.Count -= request.Quantity;
                await _variantRepository.Update(variant);

                // 8. Сохранение изменений
                var orderItemId = await _orderRepository.AddOrderItem(orderItem);
                await _orderRepository.UpdateOrderTotal(orderId, variant.Price * request.Quantity);

                _logger.LogInformation(
                    "Добавлен товар {VariantId} в заказ {OrderId}",
                    request.VariantId, orderId);

                return orderItemId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Ошибка при добавлении варианта {VariantId} в заказ {OrderId}",
                    request.VariantId, orderId);
                throw;
            }
        }
    }
}