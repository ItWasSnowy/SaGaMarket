using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SaGaMarket.Core.Storage.Repositories
{
    public interface IFavoritesRepository
    {
        Task<List<Guid>> GetUserFavorites(Guid userId);
        Task AddToFavorites(Guid userId, Guid productId);
        Task RemoveFromFavorites(Guid userId, Guid productId);
        Task<bool> IsFavorite(Guid userId, Guid productId);
    }
}