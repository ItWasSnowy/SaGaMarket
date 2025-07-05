using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
using SaGaMarket.Core.Services;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SaGaMarket.Storage.EfCore.Repository;

public class UserRepository : IUserRepository, IUserRoleService
{
    private SaGaMarketDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(SaGaMarketDbContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> Create(User user)
    {
        _context.Add(user);
        await _context.SaveChangesAsync();
        return user.UserId;
    }

    public async Task Delete(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new InvalidOperationException($"User  with ID {userId} not found.");
        }
    }

    public async Task<User?> Get(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public Task<User?> GetByEmail(string email)
    {
        throw new NotImplementedException();
    }

    public async Task Update(User user)
    {
        _context.Update(user);
        await _context.SaveChangesAsync();
    }

    public Task DisableCustomerFunctionality(Guid sellerId)
    {
        throw new NotImplementedException();
    }

    public async Task EnableCustomerFunctionality(Guid sellerId)
    {
        var user = await _context.Users.FindAsync(sellerId);
        if (user?.Role != Role.seller)
            throw new InvalidOperationException("User is not a seller");

    }

    public async Task<Role?> GetUserRoleAsync(Guid userId)
    {
        try
        {
            return await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => u.Role)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user role for {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> IsSellerWithCustomerFunctionalityAsync(Guid userId)
    {
        try
        {
            return await _context.Users
                .Where(u => u.UserId == userId &&
                           u.Role == Role.seller &&
                           u.ProductFromCartIds.Any())
                .AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking seller functionality for {UserId}", userId);
            throw;
        }
    }

   

    public async Task<bool> IsAdmin(Guid userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return user?.Role == Role.admin;
    }

    public async Task<bool> IsSeller(Guid userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return user?.Role == Role.seller;
    }

    public async Task<bool> IsCustomer(Guid userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return user?.Role == Role.customer;
    }

    public async Task<Role> GetUserRole(Guid userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return user?.Role ?? Role.customer; // Возвращаем customer по умолчанию, если пользователь не найден
    }

}


