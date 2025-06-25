using SaGaMarket.Core.Entities;
using SaGaMarket.Infrastructure.Data;
using TourGuide.Core.Storage.Repositories;

namespace SaGaMarket.Storage.EfCore.Repository;

public class UserRepository : IUserRepository
{
    private SaGaMarketDbContext _context;

    public UserRepository(SaGaMarketDbContext context)
    {
        _context = context;
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
}

