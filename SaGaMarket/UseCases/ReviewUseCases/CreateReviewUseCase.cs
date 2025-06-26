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
            var review = new Review
            {
                ProductId = request.ProductId,
                UserRating = request.UserRating,
                AuthorId = authorId,
                // Другие свойства
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
