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
    public class VariantRepository : IVariantRepository
    {
        private readonly SaGaMarketDbContext _context;

        public VariantRepository(SaGaMarketDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Create(Variant variant)
        {
            _context.Variants.Add(variant);
            await _context.SaveChangesAsync();
            return variant.VariantId;
        }

        public async Task Delete(Guid variantId)
        {
            var variant = await _context.Variants.FindAsync(variantId);
            if (variant != null)
            {
                _context.Variants.Remove(variant);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Variant?> Get(Guid variantId)
        {
            var variant = await _context.Variants
                .Include(v => v.PriceHistory)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VariantId == variantId);

            if (variant != null && variant.PriceHistory != null)
            {
                variant.PriceHistory.Price = variant.Price;
                variant.PriceHistory.VariantId = variant.VariantId;
            }

            return variant;
        }

        public async Task<IEnumerable<Variant>> GetAllForProduct(Guid productId)
        {
            return await _context.Variants
                .Where(v => v.ProductId == productId)
                .OrderBy(v => v.Price)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<List<Variant>?> GetByProduct(Guid productId)
        {
            return await _context.Variants
                .Where(v => v.ProductId == productId)
                .ToListAsync();
        }

        public async Task<bool> Update(Variant variant)
        {
            if (variant == null)
            {
                throw new ArgumentNullException(nameof(variant), "Variant cannot be null.");
            }

            var existingVariant = await _context.Variants.FindAsync(variant.VariantId);
            if (existingVariant == null)
            {
                return false;
            }

            existingVariant.Name = variant.Name;
            existingVariant.Description = variant.Description;
            existingVariant.Price = variant.Price;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while updating the variant.", ex);
            }
        }

        public async Task<bool> VariantNameExistsForProduct(Guid productId, string variantName)
        {
            return await _context.Variants
                .AnyAsync(v =>
                    v.ProductId == productId &&
                    v.Name.ToLower() == variantName.ToLower());
        }

        public async Task<List<Variant>> GetVariantsWithDetailsAsync(IEnumerable<Guid> variantIds)
        {
            return await _context.Variants
                .Where(v => variantIds.Contains(v.VariantId))
                .Include(v => v.Product)
                    .ThenInclude(p => p.Seller)
                .Include(v => v.PriceHistory)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
