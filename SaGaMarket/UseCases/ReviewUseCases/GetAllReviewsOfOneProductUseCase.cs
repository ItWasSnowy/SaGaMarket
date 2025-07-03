using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.ReviewUseCases
{
    public class GetAllReviewsOfOneProductUseCase
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductRepository _productRepository;

        public GetAllReviewsOfOneProductUseCase(
            IReviewRepository reviewRepository,
            IProductRepository productRepository)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ReviewDto>> Handle(Guid productId)
        {
            // Проверяем существует ли продукт
            var product = await _productRepository.Get(productId);
            if (product == null)
            {
                throw new InvalidOperationException("Продукт не найден");
            }

            // Получаем все отзывы для продукта
            var reviews = await _reviewRepository.GetAllForProduct(productId);

            // Преобразуем в DTO
            return reviews.Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                ProductId = r.ProductId,
                UserRating = r.UserRating,
                CommentIds = r.CommentIds,
                AuthorId = r.AuthorId,
                CreatedAt = r.CreatedAt
            });
        }

        public class ReviewDto
        {
            public Guid ReviewId { get; set; }
            public Guid ProductId { get; set; }
            public double UserRating { get; set; }
            public List<Guid> CommentIds { get; set; } = new();
            public Guid AuthorId { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}