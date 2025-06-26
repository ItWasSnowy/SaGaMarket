using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Storage.Repositories;

public interface IUserRepository
{
    public Task<Guid> Create(User user);
    public Task Update(User user);
    public Task Delete(Guid userId);

    public Task<User?> Get(Guid userId);
    public Task<User?> GetByEmail(string email);
}
