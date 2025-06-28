using Microsoft.AspNetCore.Mvc;
using SaGaMarket.Core.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static AddToCartUseCase;
using static RemoveFromCartUseCase;

[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly AddToCartUseCase _addToCartUseCase;
    private readonly RemoveFromCartUseCase _removeFromCartUseCase;
    private readonly GetUserRoleUseCase _getUserRoleUseCase;
    private readonly ILogger<CartController> _logger;

    public CartController(
        AddToCartUseCase addToCartUseCase,
        RemoveFromCartUseCase removeFromCartUseCase,
        GetUserRoleUseCase getUserRoleUseCase,
        ILogger<CartController> logger)
    {
        _addToCartUseCase = addToCartUseCase;
        _removeFromCartUseCase = removeFromCartUseCase; // Инициализация RemoveFromCartUseCase
        _getUserRoleUseCase = getUserRoleUseCase;
        _logger = logger;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart(
        [FromBody] AddToCartRequest request,
        [FromQuery] Guid userId)
    {
        try
        {
            // 1. Получаем информацию о роли пользователя
            var userRoleInfo = await _getUserRoleUseCase.Execute(userId);

            // 2. Проверяем права на добавление в корзину
            if (!userRoleInfo.CanPurchase)
            {
                return userRoleInfo.Role == Role.seller
                    ? StatusCode(403, "Продавцы должны включить функциональность покупателя, чтобы добавлять товары в корзину")
                    : StatusCode(403, "Только клиенты могут добавлять товары в корзину");
            }

            // 3. Выполняем добавление в корзину
            var result = await _addToCartUseCase.Handle(request, userId);

            return Ok(new
            {
                Success = result,
                Message = "Товар успешно добавлен в корзину",
                UserRole = userRoleInfo.RoleDescription
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ошибка валидации для пользователя {User Id}", userId);
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении в корзину для пользователя {User Id}", userId);
            return StatusCode(500, new { Error = "Внутренняя ошибка сервера" });
        }
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveFromCart(
        [FromBody] RemoveFromCartRequest request,
        [FromQuery] Guid userId)
    {
        try
        {
            // 1. Получаем информацию о роли пользователя
            var userRoleInfo = await _getUserRoleUseCase.Execute(userId);

            // 2. Проверяем права на удаление из корзины
            if (!userRoleInfo.CanPurchase)
            {
                return userRoleInfo.Role == Role.seller
                    ? StatusCode(403, "Продавцы должны включить функциональность покупателя, чтобы удалять товары из корзины")
                    : StatusCode(403, "Только клиенты могут удалять товары из корзины");
            }

            // 3. Выполняем удаление из корзины
            var result = await _removeFromCartUseCase.Handle(request, userId);

            if (!result)
            {
                return NotFound(new { Error = "Товар не найден в корзине" });
            }

            return Ok(new
            {
                Success = true,
                Message = "Товар успешно удален из корзины",
                UserRole = userRoleInfo.RoleDescription
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Ошибка валидации для пользователя {User Id}", userId);
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении из корзины для пользователя {User Id}", userId);
            return StatusCode(500, new { Error = "Внутренняя ошибка сервера" });
        }
    }
}
