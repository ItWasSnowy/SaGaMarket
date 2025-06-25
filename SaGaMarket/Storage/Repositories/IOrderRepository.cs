using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Storage.Repositories;

public interface IOrderRepository
{
    Task<string> Create(Order order);
    Task<bool> Update(Order order);
    Task Delete(string orderId);
    Task<Order?> Get(string orderId);
}
