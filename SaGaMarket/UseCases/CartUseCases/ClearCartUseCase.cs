using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.CartUseCases
{
    public class ClearCartUseCase
    {
        private readonly ICartRepository _cartRepository;

        public ClearCartUseCase(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task Handle(Guid userId)
        {
            await _cartRepository.ClearCartAsync(userId);
        }
    }
}