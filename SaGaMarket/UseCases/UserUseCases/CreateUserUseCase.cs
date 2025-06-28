using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.UserUseCases
{
    public class CreateUserUseCase
    {
        private readonly IUserRepository _userRepository;
        public CreateUserUseCase (IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task<Guid> Handle(UserDto userDto)
        {
            var user = new User(userDto);
            await _userRepository.Create(user);
            return user.UserId;
        }
    }
}
