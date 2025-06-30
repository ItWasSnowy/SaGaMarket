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
            // Проверяем, есть ли уже отзыв от этого пользователя на этот продукт
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
                CreatedAt = DateTime.UtcNow
            };

            return await _reviewRepository.Create(review);
        }

        public class CreateReviewRequest
        {
            public Guid ProductId { get; set; }
            public double UserRating { get; set; }
            // Другие свойства
        }
    }
}
