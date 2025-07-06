using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
using SaGaMarket.Storage.EfCore;

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
        try
        {
            _logger.LogDebug("Добавление заказа в контекст");
            await _context.Orders.AddAsync(order);

            _logger.LogDebug("Сохранение изменений");
            await _context.SaveChangesAsync();

            _logger.LogDebug("Заказ сохранен, ID: {OrderId}", order.OrderId);
            return order.OrderId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении заказа");
            throw;
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

    public async Task UpdateOrderTotal(Guid orderId, decimal amountToAdd)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.FinalPrice += amountToAdd;
            order.TotalPrice += amountToAdd;

            await _context.SaveChangesAsync();
        }
    }

    public async Task<IDisposable> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public async Task<IEnumerable<Order>> GetAllForUser(Guid userId)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == userId)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Variant)
            .OrderByDescending(o => o.OrderDate)
            .AsNoTracking()
            .ToListAsync();
    }

}