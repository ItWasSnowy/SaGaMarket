using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.OrderUseCases
{
    public class DeleteOrderUseCase
    {
        private readonly IOrderRepository _orderRepository;

        public DeleteOrderUseCase(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task Handle(Guid orderId, Guid customerId)
        {
            var order = await _orderRepository.Get(orderId);
            if (order == null) throw new InvalidOperationException("Order not found");
            if (order.CustomerId != customerId) throw new InvalidOperationException("Is not the customer");

            await _orderRepository.Delete(orderId);
        }
    }
}
