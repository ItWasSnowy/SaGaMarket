using Microsoft.EntityFrameworkCore;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
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

        public async Task<IEnumerable<Product>> GetFilteredProducts(
    string? category,
    double? minRating,
    string? searchTerm,
    int page,
    int pageSize)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (minRating.HasValue)
            {
                query = query.Where(p => p.AverageRating >= minRating.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm));
            }

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllCategoriesAsync()
        {
            return await _context.Products
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<int> GetFilteredProductsCount(
            string? category,
            double? minRating,
            string? searchTerm)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (minRating.HasValue)
            {
                query = query.Where(p => p.AverageRating >= minRating.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm));
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<string>> GetCategories()
        {
            return await _context.Products
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsWithPaginationAsync(
    int page,
    int pageSize)
        {
            var countQuery = _context.Products.AsNoTracking();
            var totalCount = await countQuery.CountAsync();

            var products = await _context.Products
                .Include(p => p.Variants)
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (products, totalCount);
        }

        public async Task<Product?> GetWithVariantsAsync(Guid productId)
        {
            return await _context.Products
                .Include(p => p.Variants)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productId);
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

        public async Task<List<Product>> GetProductsWithDetailsAsync(IEnumerable<Guid> productIds)
        {
            return await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .Include(p => p.Seller)
                .Include(p => p.Variants)
                .Include(p => p.Reviews)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Product>> GetByIds(List<Guid> ids)
        {
            return await _context.Products
                .Where(p => ids.Contains(p.ProductId))
                .AsNoTracking()
                .ToListAsync();
        }
        //public async Task<List<Product>> GetProductsByIdsWithDetailsAsync(IEnumerable<Guid> productIds)
        //{
        //    return await _context.Products
        //        .Where(p => productIds.Contains(p.ProductId))
        //        .Include(p => p.Seller) // Загружаем информацию о продавце
        //        .Include(p => p.Variants) // Загружаем варианты товара
        //        .Include(p => p.Tags) // Загружаем теги
        //        .AsNoTracking()
        //        .ToListAsync();
        //}
    }
}
