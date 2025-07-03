using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.ReviewUseCases
{
    public class GetProductsRatingsUseCase
    {
        private readonly IReviewRepository _reviewRepository;

        public GetProductsRatingsUseCase(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<IEnumerable<ProductRatingDto>> Handle(IEnumerable<Guid> productIds)
        {
            if (productIds == null || !productIds.Any())
            {
                return Enumerable.Empty<ProductRatingDto>();
            }

            return await _reviewRepository.GetProductsRatings(productIds);
        }
    }
    public class ProductRatingDto
    {
        public Guid ProductId { get; set; }
        public double AverageRating { get; set; }
        public int ReviewsCount { get; set; }
    }
}