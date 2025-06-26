using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.VariantUseCases
{
    public class GetVariantUseCase
    {
        private readonly IVariantRepository _variantRepository;

        public GetVariantUseCase(IVariantRepository variantRepository)
        {
            _variantRepository = variantRepository;
        }

        public async Task<Variant?> Handle(Guid variantId)
        {
            return await _variantRepository.Get(variantId);
        }
    }
}
