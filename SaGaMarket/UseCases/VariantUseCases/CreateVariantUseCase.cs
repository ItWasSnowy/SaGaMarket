using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.VariantUseCases
{
    public class CreateVariantUseCase
    {
        private readonly IVariantRepository _variantRepository;
        private readonly IProductRepository _productRepository;

        public CreateVariantUseCase(IVariantRepository variantRepository, IProductRepository productRepository)
        {
            _variantRepository = variantRepository;
            _productRepository = productRepository;
        }

        public async Task<Guid> Handle(CreateVariantRequest request)
        {
            // Проверка существования продукта
            var product = await _productRepository.Get(request.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found");
            }

            var variant = new Variant
            {
                ProductId = request.ProductId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Product = product
            };

            return await _variantRepository.Create(variant);
        }


        public class CreateVariantRequest
        {
            //public Guid VariantId { get; set; }
            public Guid ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal Price { get; set; }
        }
    }
}
