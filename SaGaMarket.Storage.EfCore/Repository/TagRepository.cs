using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SaGaMarket.Core.Entities;
using TourGuide.Core.Storage.Repositories;
using SaGaMarket.Infrastructure.Data;

namespace SaGaMarket.Storage.EfCore.Repository
{
    public class TagRepository : ITagRepository
    {
        private readonly SaGaMarketDbContext _context;

        public TagRepository(SaGaMarketDbContext context)
        {
            _context = context;
        }

        public async Task<string> Create(Tag tag)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            // Нормализуем регистр TagId
            tag.TagName = tag.TagName.ToLowerInvariant();

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            return tag.TagName;
        }

        public async Task<bool> Update(Tag tag)
        {
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            // Нормализуем регистр TagId
            tag.TagName = tag.TagName.ToLowerInvariant();

            var existingTag = await _context.Tags.FindAsync(tag.TagName);
            if (existingTag == null)
                return false;

            // Обновляем свойства (если они появятся в будущем)
            _context.Entry(existingTag).CurrentValues.SetValues(tag);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public async Task Delete(string tagId)
        {
            if (string.IsNullOrWhiteSpace(tagId))
                throw new ArgumentException("TagId cannot be null or empty", nameof(tagId));

            var normalizedTagId = tagId.ToLowerInvariant();
            var tag = await _context.Tags.FindAsync(normalizedTagId);

            if (tag != null)
            {
                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Tag?> Get(string tagId)
        {
            if (string.IsNullOrWhiteSpace(tagId))
                return null;

            var normalizedTagId = tagId.ToLowerInvariant();
            return await _context.Tags
                .FirstOrDefaultAsync(t => t.TagName == normalizedTagId);
        }

        public async Task<List<Tag>> GetAllTagsByProduct(Guid tourRouteId)
        {
            return await _context.Tags
                .Where(t => t.Products.Any(p => p.ProductId == tourRouteId))
                .ToListAsync();
        }


    }
}