using System.Xml.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SaGaMarket.Core.Dtos;

namespace SaGaMarket.Core.Entities;

public class User
{

    public Guid UserId { get; set; }
    // Роль пользователя в виде Enum
    // Пользователь должен будет выбрать какой аккаунт ему создать, аккаунт покупателя или продавца
    public Role Role { get; set; } = Role.customer; 

    //На подобии того как Тур маршрут хранит в себе список айдишников всех точек и комментов, Юзер будет хранить в себе список всех его желаемых товаров, товаров в корзине и заказанные товары.
    public List<Guid> ProductFromCartIds { get; set; } = [];
    public List<Guid> ProductFromFavoriteIds { get; set; } = [];

    public bool CanPurchase => Role == Role.customer ||
                            (Role == Role.seller && ProductFromCartIds.Any());

    public bool CanSell => Role == Role.seller;

    //name, email, passwordHash, createdAt, updatedAt будут в Identity бд


    //связи для EF

    // Navigation properties
    public List<Comment> Comments { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
    public List<Product> ProductsForSale { get; set; } = new();

    public User() { }

    public User(UserDto userDto)
    {
        UserId = userDto.UserId;
        Role = userDto.Role;
    }
}




//Пока не знаю нужны ли наследуемые классы, как будто они сюда напрашиваются. Пусть пока лежат, как идея.
public class Seller: User
{

}
public class Customer : User
{

}
public class Admin : User
{

}


public enum Role
{
    customer,
    seller,
    admin
}