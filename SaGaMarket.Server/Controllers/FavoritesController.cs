using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.UseCases;
using System;
using System.Threading.Tasks;
using static AddToFavoritesUseCase;
using static RemoveFromFavoritesUseCase;

[ApiController]
[Route("api/favorites")]
public class FavoritesController : ControllerBase
{
    private readonly AddToFavoritesUseCase _addToFavoritesUseCase;
    private readonly RemoveFromFavoritesUseCase _removeFromFavoritesUseCase;
    private readonly GetUserRoleUseCase _getUserRoleUseCase;
    private readonly GetUserFavoritesUseCase _getUserFavoritesUseCase;
    private readonly GetProductsInfoUseCase _getProductsInfoUseCase;
    private readonly ILogger<FavoritesController> _logger;

    public FavoritesController(
        AddToFavoritesUseCase addToFavoritesUseCase,
        GetProductsInfoUseCase getProductsInfoUseCase,
        GetUserFavoritesUseCase getUserFavoritesUseCase,
    RemoveFromFavoritesUseCase removeFromFavoritesUseCase,
        GetUserRoleUseCase getUserRoleUseCase,
        ILogger<FavoritesController> logger)
    {
        _addToFavoritesUseCase = addToFavoritesUseCase;
       _getUserFavoritesUseCase = getUserFavoritesUseCase;
    _removeFromFavoritesUseCase = removeFromFavoritesUseCase;
        _getProductsInfoUseCase = getProductsInfoUseCase;
        _getUserRoleUseCase = getUserRoleUseCase;
        _logger = logger;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToFavorites(
        [FromBody] AddToFavoritesRequest request,
        [FromQuery] Guid userId)
    {
        try
        {
            // 1. Получаем информацию о роли пользователя
            var userRoleInfo = await _getUserRoleUseCase.Execute(userId);

            // 2. Проверяем права на добавление в избранное
            if (!userRoleInfo.CanPurchase)
            {
                return userRoleInfo.Role == Role.seller
                    ? StatusCode(403, "Продавцы должны включить функциональность покупателя, чтобы добавлять товары в избранное")
                    : StatusCode(403, "Только клиенты могут добавлять товары в избранное");
            }

            // 3. Выполняем добавление в избранное
            var result = await _addToFavoritesUseCase.Handle(request, userId);

            return Ok(new
            {
                Success = result,
                Message = "Товар успешно добавлен в избранное",
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
            _logger.LogError(ex, "Ошибка при добавлении в избранное для пользователя {User Id}", userId);
            return StatusCode(500, new { Error = "Внутренняя ошибка сервера" });
        }
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveFromFavorites(
        [FromBody] RemoveFromFavoritesRequest request,
        [FromQuery] Guid userId)
    {
        try
        {
            var userRoleInfo = await _getUserRoleUseCase.Execute(userId);

            if (!userRoleInfo.CanPurchase)
            {
                return userRoleInfo.Role == Role.seller
                    ? StatusCode(403, "Продавцы должны включить функциональность покупателя, чтобы удалять товары из избранного")
                    : StatusCode(403, "Только клиенты могут удалять товары из избранного");
            }

            var result = await _removeFromFavoritesUseCase.Handle(request, userId);

            if (!result)
            {
                return NotFound(new { Error = "Товар не найден в избранном" });
            }

            return Ok(new
            {
                Success = true,
                Message = "Товар успешно удален из избранного",
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
            _logger.LogError(ex, "Ошибка при удалении из избранного для пользователя {User Id}", userId);
            return StatusCode(500, new { Error = "Внутренняя ошибка сервера" });
        }

    }
    [HttpGet("items")]
    public async Task<IActionResult> GetUserFavorites([FromQuery] Guid userId)
    {
        try
        {
            if (userId == Guid.Empty)
                return BadRequest("User ID must be provided");

            var favoriteItems = await _getUserFavoritesUseCase.Execute(userId);
            return Ok(favoriteItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting favorites for user {UserId}", userId);
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpGet("info")]
    public async Task<IActionResult> GetProductsInfo([FromQuery] Guid[] productIds)
    {
        try
        {
            if (productIds == null || productIds.Length == 0)
                return BadRequest("At least one product ID must be provided");

            var productsInfo = await _getProductsInfoUseCase.Execute(productIds);
            return Ok(productsInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products info");
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

}
