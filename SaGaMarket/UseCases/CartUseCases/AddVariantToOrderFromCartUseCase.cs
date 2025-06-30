using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.OrderUseCases
{
    public class AddVariantToOrderFromCartUseCase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IVariantRepository _variantRepository;

        public AddVariantToOrderFromCartUseCase(IOrderRepository orderRepository, IVariantRepository variantRepository)
        {
            _orderRepository = orderRepository;
            _variantRepository = variantRepository;
        }

        public async Task<Guid> Handle(AddVariantToOrderRequest request, Guid orderId)
        {
            // Получите вариант из репозитория
            var variant = await _variantRepository.Get(request.VariantId);
            if (variant == null)
            {
                throw new Exception("Variant not found.");
            }

            
            if (variant == null)
            {
                throw new Exception("Product not found.");
            }

            var unitPrice = variant.Price;

            var orderItem = new OrderItem
            {
                OrderId = orderId,
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                Quantity = request.Quantity,
                UnitPrice = unitPrice // Устанавливаем цену за штуку из продукта
            };


            var orderItemId = await _orderRepository.AddOrderItem(orderItem);


            await _orderRepository.UpdateOrderTotal(orderId, unitPrice * request.Quantity);

            return orderItemId;
        }



        public class AddVariantToOrderRequest
        {
            public Guid ProductId { get; set; }
            public Guid VariantId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
