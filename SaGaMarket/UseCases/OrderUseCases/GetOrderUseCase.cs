using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.OrderUseCases
{
    public class GetOrderUseCase
    {
        private readonly IOrderRepository _orderRepository;
        

        public GetOrderUseCase(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<OrderDto?> Execute(Guid orderId)
        {
            var order = await _orderRepository.Get(orderId);
            if (order == null)
                return null;

            return new OrderDto
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                TotalPrice = order.TotalPrice,
                DiscountAmount = order.DiscountAmount,
                FinalPrice = order.TotalPrice - order.DiscountAmount,
                OrderDate = order.OrderDate,
                Status = OrderStatus.Processing.ToString(),
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    OrderItemId = oi.OrderItemId,
                    ProductId = oi.ProductId,
                    VariantId = oi.VariantId,
                    ProductName = oi.Product?.Category ?? string.Empty,
                    VariantName = oi.Variant?.Name ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
            
        }
    }
}
