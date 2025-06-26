using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.ProductUseCases
{
    public class GetAllTagsByProductUseCase
    {
        private readonly ITagRepository _tagRepository;

        public GetAllTagsByProductUseCase(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<List<Tag>?> Handle(Guid productId)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("Invalid tour route ID");

            return await _tagRepository.GetAllTagsByProduct(productId);
        }
    }
}
