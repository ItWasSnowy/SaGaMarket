using Microsoft.EntityFrameworkCore;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
using SaGaMarket.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaGaMarket.Storage.EfCore.Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly SaGaMarketDbContext _context;

        public ReviewRepository(SaGaMarketDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Create(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review.ReviewId;
        }

        public async Task Delete(Guid reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Review?> Get(Guid reviewId)
        {
            return await _context.Reviews
                .Include(r => r.Comments) // Загрузка связанных комментариев
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        }

        public async Task<List<Review>?> GetByProduct(Guid productId)
        {
            return await _context.Reviews
                .Where(r => r.ProductId == productId)
                .ToListAsync();
        }

        public async Task<bool> Update(Review review)
        {
            if (review == null)
            {
                throw new ArgumentNullException(nameof(review), "Review cannot be null.");
            }

            var existingReview = await _context.Reviews.FindAsync(review.ReviewId);
            if (existingReview == null)
            {
                return false;
            }

            existingReview.UserRating = review.UserRating;
            // Обновите другие свойства по мере необходимости

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while updating the review.", ex);
            }

        }
        public async Task<bool> HasUserReviewedProduct(Guid userId, Guid productId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.AuthorId == userId && r.ProductId == productId);
        }

    }
}
