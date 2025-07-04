using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage;
using SaGaMarket.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaGaMarket.Core.Storage.Repositories
{
    public class FavoritesRepository : IFavoritesRepository
    {
        private readonly SaGaMarketDbContext _context;
        private readonly ILogger<CartRepository> _logger;

        public FavoritesRepository(SaGaMarketDbContext context, ILogger<CartRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Guid>> GetUserFavorites(Guid userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            return user?.ProductFromFavoriteIds?.ToList() ?? new List<Guid>();
        }

        public async Task AddToFavorites(Guid userId, Guid productId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new ArgumentException("User not found");

            if (!user.ProductFromFavoriteIds.Contains(productId))
            {
                user.ProductFromFavoriteIds.Add(productId);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromFavorites(Guid userId, Guid productId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new ArgumentException("User not found");

            if (user.ProductFromFavoriteIds.Contains(productId))
            {
                user.ProductFromFavoriteIds.Remove(productId);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsFavorite(Guid userId, Guid productId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);

            return user?.ProductFromFavoriteIds?.Contains(productId) ?? false;
        }
    }
}