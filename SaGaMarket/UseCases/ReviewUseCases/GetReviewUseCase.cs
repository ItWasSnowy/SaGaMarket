using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.ReviewUseCases
{
    public class GetReviewUseCase
    {
        private readonly IReviewRepository _reviewRepository;

        public GetReviewUseCase(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<Review?> Handle(Guid reviewId)
        {
            return await _reviewRepository.Get(reviewId);
        }
    }
}
