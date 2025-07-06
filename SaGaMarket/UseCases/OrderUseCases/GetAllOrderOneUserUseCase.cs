using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.OrderUseCases
{
    public class GetAllOrdersOneUserUseCase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;

        public GetAllOrdersOneUserUseCase(
            IOrderRepository orderRepository,
            IUserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<OrderDto>> Handle(Guid userId)
        {
            // Verify user exists
            var user = await _userRepository.Get(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Get all orders for the user
            var orders = await _orderRepository.GetAllForUser(userId);

            // Convert to DTO
            return orders.Select(o => new OrderDto
            {
                OrderId = o.OrderId,
                CustomerId = o.CustomerId,
                TotalPrice = o.TotalPrice,
                DiscountAmount = o.DiscountAmount,
                FinalPrice = o.FinalPrice,
                OrderDate = o.OrderDate,
                ShippedDate = o.ShippedDate,
                DeliveryDate = o.DeliveryDate,
                ShippingAddress = o.ShippingAddress,
                BillingAddress = o.BillingAddress,
                PaymentMethod = o.PaymentMethod,
                TrackingNumber = o.TrackingNumber,
                Notes = o.Notes,
                OrderStatus = o.orderStatus,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    OrderItemId = oi.OrderItemId,
                    ProductId = oi.ProductId,
                    VariantId = oi.VariantId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    OrderStatus = oi.OrderStatus
                }).ToList()
            });
        }

        public class OrderDto
        {
            public Guid OrderId { get; set; }
            public Guid CustomerId { get; set; }
            public decimal TotalPrice { get; set; }
            public decimal DiscountAmount { get; set; }
            public decimal FinalPrice { get; set; }
            public DateTime OrderDate { get; set; }
            public DateTime? ShippedDate { get; set; }
            public DateTime? DeliveryDate { get; set; }
            public string ShippingAddress { get; set; } = string.Empty;
            public string BillingAddress { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = string.Empty;
            public string TrackingNumber { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;
            public OrderStatus OrderStatus { get; set; }
            public List<OrderItemDto> OrderItems { get; set; } = new();
        }

        public class OrderItemDto
        {
            public Guid OrderItemId { get; set; }
            public Guid ProductId { get; set; }
            public Guid VariantId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public OrderItemStatus OrderStatus { get; set; }
        }
    }
}