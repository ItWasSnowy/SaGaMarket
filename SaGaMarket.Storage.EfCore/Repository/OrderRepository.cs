using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
using SaGaMarket.Infrastructure.Data;

public class OrderRepository : IOrderRepository
{
    private readonly SaGaMarketDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(SaGaMarketDbContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Guid> Create(Order order)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Order created with ID: {OrderId}", order.OrderId);
            return order.OrderId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating order");
            throw new DbUpdateException("Failed to create order", ex);
        }
    }

    public async Task Delete(Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null) return;

        try
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order with ID: {OrderId}", orderId);
            throw;
        }
    }

    public async Task<Order?> Get(Guid orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Variant)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<List<Order>> GetByCustomer(Guid customerId)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.OrderItems)
            .ToListAsync();
    }

    public async Task<bool> Update(Order order)
    {
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order), "Order cannot be null.");
        }

        var existingOrder = await _context.Orders.FindAsync(order.OrderId);
        if (existingOrder == null)
        {
            return false;
        }

        existingOrder.TotalPrice = order.TotalPrice;

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occurred while updating the order.", ex);
        }

    }
    public async Task<Guid> AddOrderItem(OrderItem orderItem)
    {
        _context.OrderItems.Add(orderItem);
        await _context.SaveChangesAsync();
        return orderItem.OrderItemId;
    }
}