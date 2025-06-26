using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.ProductUseCases
{
    public class DeleteProductUseCase
    {
        private readonly IProductRepository _productRepository;

        public DeleteProductUseCase(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task Handle(Guid productId, Guid sellerId)
        {
            var product = await _productRepository.Get(productId);
            if (product == null) throw new InvalidOperationException("Product not found");
            if (product.SellerId != sellerId) throw new InvalidOperationException("Is not the seller");

            await _productRepository.Delete(productId);
        }
    }
}
