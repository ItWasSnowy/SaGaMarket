using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.ReviewUseCases
{
    public class UpdateReviewUseCase
    {
        private readonly IReviewRepository _reviewRepository;

        public UpdateReviewUseCase(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task Handle(Guid reviewId, double newRating, Guid authorId)
        {
            var existingReview = await _reviewRepository.Get(reviewId);
            if (existingReview == null)
                throw new ArgumentException("Review not found");

            if (existingReview.AuthorId != authorId)
                throw new UnauthorizedAccessException("You can only update your own reviews");

            existingReview.UserRating = newRating;

            if (!await _reviewRepository.Update(existingReview))
                throw new Exception("Failed to update review");
        }
    }
}
