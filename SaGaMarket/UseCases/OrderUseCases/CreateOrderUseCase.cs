using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.OrderUseCases
{
    public class CreateOrderUseCase
    {
        private readonly IOrderRepository _orderRepository;

        public CreateOrderUseCase(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Guid> Handle(CreateOrderRequest request, Guid customerId)
        {
            var order = new Order
            {
                CustomerId = customerId,
                TotalPrice = request.TotalPrice,
                // Другие свойства
            };
            return await _orderRepository.Create(order);
        }

        public class CreateOrderRequest
        {
            public decimal TotalPrice { get; set; }
            // Другие свойства
        }
    }
}
