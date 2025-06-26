using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.ReviewUseCases
{
    public class DeleteReviewUseCase
    {
        private readonly IReviewRepository _reviewRepository;

        public DeleteReviewUseCase(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task Handle(Guid reviewId, Guid authorId)
        {
            var review = await _reviewRepository.Get(reviewId);
            if (review == null) throw new InvalidOperationException("Review not found");
            if (review.AuthorId != authorId) throw new InvalidOperationException("Is not the author");

            await _reviewRepository.Delete(reviewId);
        }
    }
}
