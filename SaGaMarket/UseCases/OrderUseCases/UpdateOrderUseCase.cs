using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.OrderUseCases
{
    public class UpdateOrderUseCase
    {
        private readonly IOrderRepository _orderRepository;

        public UpdateOrderUseCase(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Handle(Guid orderId, UpdateOrderRequest request, Guid customerId)
        {
            var existingOrder = await _orderRepository.Get(orderId);
            if (existingOrder == null)
                throw new ArgumentException("Order not found");

            if (existingOrder.CustomerId != customerId)
                throw new UnauthorizedAccessException("You can only update your own orders");

            existingOrder.TotalPrice = request.TotalPrice;

            if (!await _orderRepository.Update(existingOrder))
                throw new Exception("Failed to update order");
        }

        public class UpdateOrderRequest
        {
            public decimal TotalPrice { get; set; }
        }
    }
}
