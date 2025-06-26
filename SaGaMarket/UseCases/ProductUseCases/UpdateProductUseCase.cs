using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.ProductUseCases
{
    public class UpdateProductUseCase
    {
        private readonly IProductRepository _productRepository;

        public UpdateProductUseCase(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task Handle(Guid productId, UpdateProductRequest request, Guid sellerId)
        {
            var existingProduct = await _productRepository.Get(productId);
            if (existingProduct == null)
                throw new ArgumentException("Product not found");

            if (existingProduct.SellerId != sellerId)
                throw new UnauthorizedAccessException("You can only update your own products");

            existingProduct.Category = request.Category;
            // Обновите другие свойства

            if (!await _productRepository.Update(existingProduct))
                throw new Exception("Failed to update product");
        }

        public class UpdateProductRequest
        {
            public string Category { get; set; } = string.Empty;
            // Другие свойства
        }
    }
}
