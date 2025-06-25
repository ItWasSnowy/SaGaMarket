namespace SaGaMarket.Core.Entities;

public class Order
{

    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalPrice { get; set; }
   


    public User Customer { get; set; }
    public List<Product> Products { get; set; } = new();

}
