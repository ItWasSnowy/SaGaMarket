using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.VariantUseCases
{
    public class GetAllVariantsOfOneProductUseCase
    {
        private readonly IVariantRepository _variantRepository;
        private readonly IProductRepository _productRepository;

        public GetAllVariantsOfOneProductUseCase(
            IVariantRepository variantRepository,
            IProductRepository productRepository)
        {
            _variantRepository = variantRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<VariantDto>> Handle(Guid productId)
        {
            // Проверка существования продукта
            var product = await _productRepository.Get(productId);
            if (product == null)
            {
                throw new InvalidOperationException("Product not found");
            }

            var variants = await _variantRepository.GetAllForProduct(productId);

            return variants.Select(v => new VariantDto
            {
                VariantId = v.VariantId,
                ProductId = v.ProductId,
                Name = v.Name,
                Description = v.Description,
                Price = v.Price,
                Count = v.Count
            });
        }

        public class VariantDto
        {
            public Guid VariantId { get; set; }
            public Guid ProductId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public int Count { get; set; }
        }
    }
}