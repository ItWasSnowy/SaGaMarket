using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.UseCases;
using SaGaMarket.Identity;
using SaGaMarket.Server.Identity;
using System;
using System.Threading.Tasks;
using static AddToFavoritesUseCase;
using static RemoveFromFavoritesUseCase;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly AddToFavoritesUseCase _addToFavoritesUseCase;
    private readonly RemoveFromFavoritesUseCase _removeFromFavoritesUseCase;
    private readonly GetUserRoleUseCase _getUserRoleUseCase;
    private readonly GetUserFavoritesUseCase _getUserFavoritesUseCase;
    private readonly GetProductsInfoUseCase _getProductsInfoUseCase;
    private readonly ILogger<FavoritesController> _logger;
    private readonly UserManager<SaGaMarketIdentityUser> _userManager;

    public FavoritesController(
        AddToFavoritesUseCase addToFavoritesUseCase,
        GetProductsInfoUseCase getProductsInfoUseCase,
        GetUserFavoritesUseCase getUserFavoritesUseCase,
        RemoveFromFavoritesUseCase removeFromFavoritesUseCase,
        GetUserRoleUseCase getUserRoleUseCase,
        ILogger<FavoritesController> logger,
        UserManager<SaGaMarketIdentityUser> userManager)
    {
        _addToFavoritesUseCase = addToFavoritesUseCase;
        _getUserFavoritesUseCase = getUserFavoritesUseCase;
        _removeFromFavoritesUseCase = removeFromFavoritesUseCase;
        _getProductsInfoUseCase = getProductsInfoUseCase;
        _getUserRoleUseCase = getUserRoleUseCase;
        _logger = logger;
        _userManager = userManager;
    }

    [HttpPost("add")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> AddToFavorites([FromBody] AddToFavoritesRequest request)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User));

        try
        {
            var userRoleInfo = await _getUserRoleUseCase.Execute(userId);

            if (!userRoleInfo.CanPurchase)
            {
                return userRoleInfo.Role == Role.seller
                    ? StatusCode(403, new { Error = "Продавцы должны включить функциональность покупателя, чтобы добавлять товары в избранное" })
                    : StatusCode(403, new { Error = "Только клиенты могут добавлять товары в избранное" });
            }

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
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> RemoveFromFavorites([FromBody] RemoveFromFavoritesRequest request)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User));

        try
        {
            var userRoleInfo = await _getUserRoleUseCase.Execute(userId);

            if (!userRoleInfo.CanPurchase)
            {
                return userRoleInfo.Role == Role.seller
                    ? StatusCode(403, new { Error = "Продавцы должны включить функциональность покупателя, чтобы удалять товары из избранного" })
                    : StatusCode(403, new { Error = "Только клиенты могут удалять товары из избранного" });
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
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> GetUserFavorites()
    {
        var userId = Guid.Parse(_userManager.GetUserId(User));

        try
        {
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
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> GetProductsInfo([FromQuery] Guid[] productIds)
    {
        try
        {
            if (productIds == null || productIds.Length == 0)
                return BadRequest(new { Error = "At least one product ID must be provided" });

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