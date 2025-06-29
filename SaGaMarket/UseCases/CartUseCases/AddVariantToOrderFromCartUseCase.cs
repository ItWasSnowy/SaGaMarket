using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.OrderUseCases
{
    public class AddVariantToOrderFromCartUseCase
    {
        private readonly IOrderRepository _orderRepository;

        public AddVariantToOrderFromCartUseCase(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Guid> Handle(AddVariantToOrderRequest request, Guid orderId)
        {
            var orderItem = new OrderItem
            {
                OrderId = orderId,
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice
            };

            return await _orderRepository.AddOrderItem(orderItem);
        }

        public class AddVariantToOrderRequest
        {
            public Guid ProductId { get; set; }
            public Guid VariantId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }
    }
}
