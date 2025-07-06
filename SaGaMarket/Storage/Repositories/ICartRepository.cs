using SaGaMarket.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaGaMarket.Core.Storage.Repositories
{
    public interface ICartRepository
    {
        Task<User?> GetUserWithCart(Guid userId);
        Task<bool> SaveChangesAsync();
        Task<List<Guid>> GetUserCartItems(Guid userId);
        Task ClearCartAsync(Guid userId);
    }
}
