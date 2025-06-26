using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.UserUseCases
{
    public class GetByEmailUserUseCase
    {
        private readonly IUserRepository _userRepository;
        public GetByEmailUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<UserDto?> Handle(string email)
        {
            var user = await _userRepository.GetByEmail(email);
            if (user == null) return null;
            return new UserDto(user);
        }
    }
}
