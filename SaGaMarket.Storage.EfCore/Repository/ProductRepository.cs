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
    public class ProductRepository : IProductRepository
    {
        private readonly SaGaMarketDbContext _context;

        public ProductRepository(SaGaMarketDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Create(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product.ProductId;
        }

        public async Task Delete(Guid productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Product?> Get(Guid productId)
        {
            return await _context.Products
                .Include(p => p.Variants) 
                .Include(p => p.Reviews) 
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public async Task<Product?> Get(Guid productId, bool includeTags = false)
        {
            if (includeTags)
            {
                return await _context.Products
                    .Include(tr => tr.Tags)
                    .FirstOrDefaultAsync(tr => tr.ProductId == productId);
            }
            return await _context.Products.FindAsync(productId);
        }

        public async Task<List<Product>?> GetBySeller(Guid sellerId)
        {
            return await _context.Products
                .Where(p => p.SellerId == sellerId)
                .ToListAsync();
        }

        public async Task<bool> Update(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "Product cannot be null.");
            }

            var existingProduct = await _context.Products.FindAsync(product.ProductId);
            if (existingProduct == null)
            {
                return false;
            }

            existingProduct.Category = product.Category;
            existingProduct.AverageRating = product.AverageRating;
            // Обновите другие свойства по мере необходимости

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while updating the product.", ex);
            }
        }
    }
}
