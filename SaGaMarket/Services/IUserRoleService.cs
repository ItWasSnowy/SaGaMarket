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
    }
}
