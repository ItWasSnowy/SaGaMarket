using SaGaMarket.Core.Entities;
using SaGaMarket.Core.UseCases.ReviewUseCases;

namespace SaGaMarket.Core.Storage.Repositories;

public interface IReviewRepository
{
    Task<Guid> Create(Review review);
    Task<bool> Update(Review review);
    Task Delete(Guid reviewId);
    Task<Review?> Get(Guid reviewId);
    Task<bool> HasUserReviewedProduct(Guid userId, Guid productId);
    Task<IEnumerable<Review>> GetAllForProduct(Guid productId);
    Task<IEnumerable<ProductRatingDto>> GetProductsRatings(IEnumerable<Guid> productIds);
}
