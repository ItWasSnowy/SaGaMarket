using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.TagUseCases
{
    public class RemoveTagFromProductUseCase
    {
        private readonly IProductRepository _productRepository;
        private readonly ITagRepository _tagRepository;

        public RemoveTagFromProductUseCase(
            IProductRepository productRepository,
            ITagRepository tagRepository)
        {
            _productRepository = productRepository;
            _tagRepository = tagRepository;
        }
       
        public async Task RemoveTagFromTourRoute(Guid productId, string tagId)
        {
            if (string.IsNullOrWhiteSpace(tagId))
                throw new ArgumentException("Tag ID cannot be empty");

            var product = await _productRepository
                .Get(productId, includeTags: true);

            if (product == null)
                throw new InvalidOperationException("Tour route not found");

            var tagToRemove = product.Tags
                .FirstOrDefault(t => t.TagId.Equals(tagId, StringComparison.OrdinalIgnoreCase));

            if (tagToRemove != null)
            {
                product.Tags.Remove(tagToRemove);
                await _productRepository.Update(product);
            }
        }
    }
}
