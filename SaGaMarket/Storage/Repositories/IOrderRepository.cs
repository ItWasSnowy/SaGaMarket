using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Storage.Repositories;

public interface IOrderRepository
{
    Task<Guid> Create(Order order);
    Task<bool> Update(Order order);
    Task Delete(Guid orderId);
    Task<Order?> Get(Guid orderId);
    Task<Guid> AddOrderItem(OrderItem orderItem);
    Task UpdateOrderTotal(Guid orderId, decimal amountToAdd);

    Task<IDisposable> BeginTransactionAsync();
}
