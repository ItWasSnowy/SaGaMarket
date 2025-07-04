using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Storage.Repositories;

public interface IVariantRepository
{
    Task<Guid> Create(Variant variant);
    Task<bool> Update(Variant variant);
    Task Delete(Guid variantId);
    Task<Variant?> Get(Guid variantId);
    Task<IEnumerable<Variant>> GetAllForProduct(Guid productId);
    Task<bool> VariantNameExistsForProduct(Guid productId, string variantName);
    Task<List<Variant>> GetVariantsWithDetailsAsync(IEnumerable<Guid> variantIds);
}
