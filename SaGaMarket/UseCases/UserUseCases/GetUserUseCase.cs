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
    public class GetUserUseCase
    {
        private readonly IUserRepository _userRepository;
        public GetUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<UserDto?> Handle(Guid userId)
        {
            var user = await _userRepository.Get(userId);
            if (user == null) return null;
            return new UserDto(user);
        }
    }
}
