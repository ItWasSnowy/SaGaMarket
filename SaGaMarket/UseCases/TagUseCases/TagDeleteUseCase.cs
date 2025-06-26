using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.Tags
{
    public class DeleteTagUseCase
    {
        private readonly ITagRepository _tagRepository;

        public DeleteTagUseCase(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task Handle(TagDto tagDto)
        {
            if (string.IsNullOrWhiteSpace(tagDto.TagId))
                throw new ArgumentException("Tag ID cannot be empty");

            var tagId = tagDto.TagId.ToLowerInvariant().Trim();

            await _tagRepository.Delete(tagId);
        }
    }
}