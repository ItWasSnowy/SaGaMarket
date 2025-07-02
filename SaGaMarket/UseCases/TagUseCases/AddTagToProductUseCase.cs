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
            Tag tagTr = await _tagRepository.Get(tagId);

            if (tagTr == null)
            {
                tagTr = new Tag(tagDto);
                await _tagRepository.Create(tagTr);
            }

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