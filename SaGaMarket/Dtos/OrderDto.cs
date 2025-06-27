using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Dtos;
public class OrderDto
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; }
    public string ShippingAddress { get; set; }
    public string PaymentMethod { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public Guid OrderItemId { get; set; }
    public Guid ProductId { get; set; }
    public Guid VariantId { get; set; }
    public string ProductName { get; set; }
    public string VariantName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}