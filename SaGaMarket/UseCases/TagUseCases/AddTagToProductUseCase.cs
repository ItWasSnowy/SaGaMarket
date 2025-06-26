using System.Linq;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.Tags
{
    public class AddTagToProductUseCase
    {
        private readonly IProductRepository _productRepository;
        private readonly ITagRepository _tagRepository;

        public AddTagToProductUseCase(
            IProductRepository productRepository,
            ITagRepository tagRepository)
        {
            _productRepository = productRepository;
            _tagRepository = tagRepository;
        }

        public async Task Execute(Guid tourRouteId, TagDto tagDto)
        {
            if (string.IsNullOrWhiteSpace(tagDto.TagId))
                throw new ArgumentException("Tag ID cannot be empty");

            var tagId = tagDto.TagId.ToLowerInvariant().Trim();
            Tag tagTr = await _tagRepository.Get(tagId); // Retrieve the existing tag

            if (tagTr == null)
            {
                tagTr = new Tag(tagDto); // Create a new tag if it doesn't exist
                await _tagRepository.Create(tagTr);
            }
            // No need to create a new Tag instance if it already exists

            var tourRoute = await _productRepository.Get(tourRouteId);
            if (tourRoute == null)
                throw new InvalidOperationException("Tour route not found");

            if (!tourRoute.Tags.Contains(tagTr))
            {
                tourRoute.Tags.Add(tagTr);
                await _productRepository.Update(tourRoute);
            }
        }
    }
}