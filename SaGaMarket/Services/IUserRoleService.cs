using SaGaMarket.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaGaMarket.Core.Services
{
    public interface IUserRoleService
    {
        Task EnableCustomerFunctionality(Guid sellerId);
        Task DisableCustomerFunctionality(Guid sellerId);
        Task<bool> IsAdmin(Guid userId);
        Task<bool> IsSeller(Guid userId);
        Task<bool> IsCustomer(Guid userId);
        Task<Role> GetUserRole(Guid userId);
    }
}
