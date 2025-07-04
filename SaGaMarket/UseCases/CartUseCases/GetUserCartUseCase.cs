using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SaGaMarket.Core.UseCases
{
    public class GetUserCartUseCase
    {
        private readonly ICartRepository _cartRepository;

        public GetUserCartUseCase(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<List<Guid>> Execute(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID");

            var cartItems = await _cartRepository.GetUserCartItems(userId);
            return cartItems;
        }
    }
}