using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Dtos;

public  class UserDto
{
    public Guid UserId { get; set; }
    public Role Role { get; set; } = Role.customer;

    public UserDto() { } 
    public UserDto(User user) 
    {
        UserId = user.UserId;
        Role = user.Role;
    }
}
