using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.VariantUseCases
{
    public class UpdateCountVariantUseCase
    {
        private readonly IVariantRepository _variantRepository;

        public UpdateCountVariantUseCase(IVariantRepository variantRepository)
        {
            _variantRepository = variantRepository;
        }

        public async Task Handle(Guid variantId, UpdateCountVariantRequest request)
        {
            var existingVariant = await _variantRepository.Get(variantId);
            if (existingVariant == null)
                throw new ArgumentException("Variant not found");

            existingVariant.Count = request.Count;

            if (!await _variantRepository.Update(existingVariant))
                throw new Exception("Failed to update variant");
        }

        public class UpdateCountVariantRequest
        {
            public int Count { get; set; }
        }
    }
}
