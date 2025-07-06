using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SaGaMarket.Core.UseCases.OrderUseCases;
using SaGaMarket.Identity;
using SaGaMarket.Server.Identity;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using static CreateOrderUseCase;
using static SaGaMarket.Core.UseCases.OrderUseCases.AddVariantToOrderFromCartUseCase;
using static SaGaMarket.Core.UseCases.OrderUseCases.UpdateOrderUseCase;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly CreateOrderUseCase _createOrderUseCase;
    private readonly GetOrderUseCase _getOrderUseCase;
    private readonly UpdateOrderUseCase _updateOrderUseCase;
    private readonly DeleteOrderUseCase _deleteOrderUseCase;
    private readonly AddVariantToOrderFromCartUseCase _addVariantToOrderFromCartUseCase;
    private readonly UserManager<SaGaMarketIdentityUser> _userManager;
    private readonly GetAllOrdersOneUserUseCase _getAllOrdersOneUserUseCase;

    public OrderController(
        AddVariantToOrderFromCartUseCase addVariantToOrderFromCartUseCase,
        GetAllOrdersOneUserUseCase getAllOrdersOneUserUseCase,
        CreateOrderUseCase createOrderUseCase,
        GetOrderUseCase getOrderUseCase,
        UpdateOrderUseCase updateOrderUseCase,
        DeleteOrderUseCase deleteOrderUseCase,
        UserManager<SaGaMarketIdentityUser> userManager)
    {
        _addVariantToOrderFromCartUseCase = addVariantToOrderFromCartUseCase;
        _getAllOrdersOneUserUseCase = getAllOrdersOneUserUseCase;
        _createOrderUseCase = createOrderUseCase;
        _getOrderUseCase = getOrderUseCase;
        _updateOrderUseCase = updateOrderUseCase;
        _deleteOrderUseCase = deleteOrderUseCase;
        _userManager = userManager;
    }

    [HttpPost]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var customerId = Guid.Parse(_userManager.GetUserId(User));
            var orderId = await _createOrderUseCase.Handle(request, customerId);
            return Ok(new { OrderId = orderId, Message = "Заказ успешно создан" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Внутренняя ошибка сервера. Пожалуйста, попробуйте позже." });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> GetOrderDetails(Guid id)
    {
        try
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var order = await _getOrderUseCase.Execute(id);

            if (order == null)
                return NotFound(new { Error = "Order not found" });

            // Проверка что пользователь имеет доступ к заказу
            if (order.CustomerId != userId && !User.IsInRole("admin"))
                return Forbid();

            return Ok(order);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateOrderRequest request)
    {
        try
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _updateOrderUseCase.Handle(id, request, userId);
            return Ok(new { Message = "Order updated successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _deleteOrderUseCase.Handle(id, userId);
            return Ok(new { Message = "Order deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpPost("{orderId}/add-variant")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> AddVariantToOrder(
    [FromBody] AddVariantToOrderRequest request,
    [FromRoute] Guid orderId)
    {
        try
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var orderItemId = await _addVariantToOrderFromCartUseCase.Handle(request, orderId, userId);

            return Ok(new
            {
                OrderItemId = orderItemId,
                Message = "Товар успешно добавлен в заказ"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Внутренняя ошибка сервера" });
        }
    }
    [HttpGet("my-orders")]
    [Authorize] // Требуется авторизация
    public async Task<IActionResult> GetCurrentUserOrders()
    {
        try
        {
            // Получаем ID текущего авторизованного пользователя
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Error = "Пользователь не идентифицирован" });
            }

            var currentUserId = Guid.Parse(userId);

            // Получаем заказы только для текущего пользователя
            var orders = await _getAllOrdersOneUserUseCase.Handle(currentUserId);
            return Ok(orders);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (FormatException)
        {
            return BadRequest(new { Error = "Неверный формат идентификатора пользователя" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Внутренняя ошибка сервера" });
        }
    }
}