using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Storage.Repositories;

public interface IProductRepository
{
    Task<Guid> Create(Product product);
    Task<bool> Update(Product product);
    Task Delete(Guid productId);
    Task<Product?> Get(Guid productId);
    Task<Product> Get(Guid productId, bool includeTags = false);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsWithPaginationAsync(int page, int pageSize);
    Task<List<Product>> GetProductsWithDetailsAsync(IEnumerable<Guid> productIds);
    Task<List<Product>> GetByIds(List<Guid> ids);
    Task<List<string>> GetAllCategoriesAsync();
    Task<IEnumerable<string>> GetCategories();
    Task<int> GetFilteredProductsCount(
            string? category,
            double? minRating,
            string? searchTerm);
    Task<IEnumerable<Product>> GetFilteredProducts(
    string? category,
    double? minRating,
    string? searchTerm,
    int page,
    int pageSize);


}
