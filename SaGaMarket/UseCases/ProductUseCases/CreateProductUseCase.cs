using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.ProductUseCases
{
    public class CreateProductUseCase
    {
        private readonly IProductRepository _productRepository;

        public CreateProductUseCase(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Guid> Handle(CreateProductRequest request, Guid sellerId)
        {
            var product = new Product
            {
                SellerId = sellerId,
                Category = request.Category,
                AverageRating = 0,
                
            };
            return await _productRepository.Create(product);
        }

        public class CreateProductRequest
        {
            public string Category { get; set; } = string.Empty;
            
        }
    }
}
