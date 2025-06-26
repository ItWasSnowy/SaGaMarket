using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace TourGuide.Core.UseCases.TagUseCases
{
    public class GetTagUseCase 
    {
        private readonly ITagRepository _tagRepository;

        public GetTagUseCase(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<TagDto?> Handle(string tagId)
        {
            var tag = await _tagRepository.Get(tagId);
            if (tag == null) return null;

            return new TagDto(tag);
        }
    }
    
}
