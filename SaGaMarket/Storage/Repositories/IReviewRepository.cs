using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Storage.Repositories;

public interface IReviewRepository
{
    Task<string> Create(Review review);
    Task<bool> Update(Review review);
    Task Delete(string reviewId);
    Task<Review?> Get(string reviewId);
}
