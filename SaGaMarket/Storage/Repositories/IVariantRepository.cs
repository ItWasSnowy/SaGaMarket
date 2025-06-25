using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Storage.Repositories;

public interface IVariantRepository
{
    Task<string> Create(Variant variant);
    Task<bool> Update(Variant variant);
    Task Delete(string variantId);
    Task<Variant?> Get(string variantId);
}
