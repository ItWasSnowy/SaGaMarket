using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.VariantUseCases
{
    public class CreateVariantUseCase
    {
        private readonly IVariantRepository _variantRepository;

        public CreateVariantUseCase(IVariantRepository variantRepository)
        {
            _variantRepository = variantRepository;
        }

        public async Task<Guid> Handle(CreateVariantRequest request)
        {
            var variant = new Variant
            {
                ProductId = request.ProductId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
            };
            return await _variantRepository.Create(variant);
        }

        public class CreateVariantRequest
        {
            public Guid ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal Price { get; set; }
        }
    }
}
