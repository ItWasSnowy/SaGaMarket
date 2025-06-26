using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.VariantUseCases
{
    public class DeleteVariantUseCase
    {
        private readonly IVariantRepository _variantRepository;

        public DeleteVariantUseCase(IVariantRepository variantRepository)
        {
            _variantRepository = variantRepository;
        }

        public async Task Handle(Guid variantId)
        {
            var variant = await _variantRepository.Get(variantId);
            if (variant == null) throw new InvalidOperationException("Variant not found");

            await _variantRepository.Delete(variantId);
        }
    }
}
