using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
using SaGaMarket.Infrastructure.Data;
using System;

public class CartRepository : ICartRepository
{
    private readonly SaGaMarketDbContext _context;
    private readonly ILogger<CartRepository> _logger;

    public CartRepository(SaGaMarketDbContext context, ILogger<CartRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetUserWithCart(Guid userId)
    {
        return await _context.Users
            .Include(u => u.ProductsForSale)
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<bool> SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving cart changes");
            return false;
        }
    }
    public async Task<List<Guid>> GetUserCartItems(Guid userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return user?.ProductFromCartIds ?? new List<Guid>();
    }

}