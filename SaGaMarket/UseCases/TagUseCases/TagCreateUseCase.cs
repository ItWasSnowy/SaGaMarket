using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.Tags
{
    public class CreateTagUseCase
    {
        private readonly ITagRepository _tagRepository;

        public CreateTagUseCase(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<TagDto> Handle(TagDto tagDto)
        {
            if (string.IsNullOrWhiteSpace(tagDto.TagId))
                throw new ArgumentException("Tag ID cannot be empty");

            var tagId = tagDto.TagId.ToLowerInvariant().Trim();

            //if (await _tagRepository.Exists(tagId))
            //    throw new InvalidOperationException($"Tag '{tagId}' already exists");

            var createdTagId = await _tagRepository.Create(new Tag(tagId));
            return new TagDto(createdTagId);
        }
    }
}