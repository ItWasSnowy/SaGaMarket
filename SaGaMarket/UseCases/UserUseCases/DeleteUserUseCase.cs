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
    public class DeleteUserUseCase
    {
        private readonly IUserRepository _userRepository;
        public DeleteUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        public async Task Handle(Guid userId, Guid currentUserId)
        {
            var u = await _userRepository.Get(userId);
            if (u is null) throw new InvalidOperationException("User not found");
            if (u.UserId != currentUserId) throw new InvalidOperationException("You not this person");
            await _userRepository.Delete(userId);
            //return await _userRepository.Create(userDto);
        }
    }
}
