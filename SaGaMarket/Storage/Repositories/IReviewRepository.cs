using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Storage.Repositories;

public interface IReviewRepository
{
    Task<Guid> Create(Review review);
    Task<bool> Update(Review review);
    Task Delete(Guid reviewId);
    Task<Review?> Get(Guid reviewId);
}
