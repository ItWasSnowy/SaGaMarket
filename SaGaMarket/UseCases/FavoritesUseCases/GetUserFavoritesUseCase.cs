using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SaGaMarket.Core.UseCases
{
    public class GetUserFavoritesUseCase
    {
        private readonly IFavoritesRepository _favoritesRepository;

        public GetUserFavoritesUseCase(IFavoritesRepository favoritesRepository)
        {
            _favoritesRepository = favoritesRepository;
        }

        public async Task<List<Guid>> Execute(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID");

            return await _favoritesRepository.GetUserFavorites(userId);
        }
    }
}