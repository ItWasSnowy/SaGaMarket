using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
using System;
using System.Threading.Tasks;

namespace SaGaMarket.Core.UseCases.TagUseCases
{
    public class TagCreateUseCase
    {
        private readonly ITagRepository _tagRepository;

        public TagCreateUseCase(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<string> Handle(CreateTagRequest request)
        {
            var tagDto = new TagDto
            {
                TagId = request.TagId
            };

           
            var tag = new Tag(tagDto);

            
            var createdTagId = await _tagRepository.Create(tag);
            return createdTagId;
        }
        
    }
    public class CreateTagRequest
    {
        public string TagId { get; set; } = string.Empty;
    }

}
