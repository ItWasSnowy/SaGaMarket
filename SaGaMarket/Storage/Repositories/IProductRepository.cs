using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Storage.Repositories;

public interface IProductRepository
{
    Task<string> Create(Product product);
    Task<bool> Update(Product product);
    Task Delete(string productId);
    Task<Product?> Get(string productId);
}
