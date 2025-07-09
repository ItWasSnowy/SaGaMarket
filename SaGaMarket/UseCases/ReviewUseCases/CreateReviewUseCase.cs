using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.ReviewUseCases
{
    public class CreateReviewUseCase
    {
        private readonly IReviewRepository _reviewRepository;

        public CreateReviewUseCase(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<Guid> Handle(CreateReviewRequest request, Guid authorId)
        {
            bool hasExistingReview = await _reviewRepository.HasUserReviewedProduct(authorId, request.ProductId);

            if (hasExistingReview)
            {
                throw new InvalidOperationException("Вы уже оставили отзыв на этот продукт");
            }

            var review = new Review
            {
                ProductId = request.ProductId,
                UserRating = request.UserRating,
                AuthorId = authorId,
                TextReview = request.TextReview,
                CreatedAt = DateTime.UtcNow
            };

            return await _reviewRepository.Create(review);
        }

        public class CreateReviewRequest
        {
            public Guid ProductId { get; set; }
            public string? TextReview { get; set; }
            public double UserRating { get; set; }
        }
    }
}
