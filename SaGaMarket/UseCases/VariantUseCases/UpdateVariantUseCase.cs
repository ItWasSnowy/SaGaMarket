using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.VariantUseCases
{
    public class UpdateVariantUseCase
    {
        private readonly IVariantRepository _variantRepository;

        public UpdateVariantUseCase(IVariantRepository variantRepository)
        {
            _variantRepository = variantRepository;
        }

        public async Task Handle(Guid variantId, UpdateVariantRequest request)
        {
            var existingVariant = await _variantRepository.Get(variantId);
            if (existingVariant == null)
                throw new ArgumentException("Variant not found");

            existingVariant.Name = request.Name;
            existingVariant.Description = request.Description;
            existingVariant.Price = request.Price;

            if (!await _variantRepository.Update(existingVariant))
                throw new Exception("Failed to update variant");
        }

        public class UpdateVariantRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal Price { get; set; }
        }
    }
}
