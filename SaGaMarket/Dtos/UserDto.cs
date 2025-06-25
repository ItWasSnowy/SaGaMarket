using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Dtos;

public  class UserDto
{
    public Guid UserId { get; set; }
    public Role Role { get; set; } = Role.customer;
    public List<Guid> ProductFromCartIds { get; set; } = [];
    public List<Guid> ProductFromFavoriteIds { get; set; } = [];
    public List<Guid> ProductFromOrderIds { get; set; } = [];


    public UserDto() { } 
    public UserDto(User user) 
    {
        UserId = user.UserId;
        Role = user.Role;
        ProductFromCartIds = user.ProductFromCartIds;
        ProductFromFavoriteIds = user.ProductFromFavoriteIds;
        ProductFromOrderIds = user.ProductFromFavoriteIds;
    }
}
