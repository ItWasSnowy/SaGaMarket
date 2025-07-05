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
using static AddToCartUseCase;
using static RemoveFromCartUseCase;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AddToCartUseCase _addToCartUseCase;
    private readonly RemoveFromCartUseCase _removeFromCartUseCase;
    private readonly GetUserRoleUseCase _getUserRoleUseCase;
    private readonly GetUserCartUseCase _getUserCartUseCase;
    private readonly GetCartItemsInfoUseCase _getCartItemsInfoUseCase;
    private readonly ILogger<CartController> _logger;
    private readonly UserManager<SaGaMarketIdentityUser> _userManager;

    public CartController(
        AddToCartUseCase addToCartUseCase,
        GetCartItemsInfoUseCase getCartItemsInfoUseCase,
        GetUserCartUseCase getUserCartUseCase,
        RemoveFromCartUseCase removeFromCartUseCase,
        GetUserRoleUseCase getUserRoleUseCase,
        ILogger<CartController> logger,
        UserManager<SaGaMarketIdentityUser> userManager)
    {
        _addToCartUseCase = addToCartUseCase;
        _removeFromCartUseCase = removeFromCartUseCase;
        _getUserRoleUseCase = getUserRoleUseCase;
        _getUserCartUseCase = getUserCartUseCase;
        _getCartItemsInfoUseCase = getCartItemsInfoUseCase;
        _logger = logger;
        _userManager = userManager;
    }

    [HttpPost("add")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User));

        try
        {
            var userRoleInfo = await _getUserRoleUseCase.Execute(userId);

            if (!userRoleInfo.CanPurchase)
            {
                return userRoleInfo.Role == Role.seller
                    ? StatusCode(403, new { Error = "Продавцы должны включить функциональность покупателя, чтобы добавлять товары в корзину" })
                    : StatusCode(403, new { Error = "Только клиенты могут добавлять товары в корзину" });
            }

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

    [HttpGet("items")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> GetUserCartItems()
    {
        var userId = Guid.Parse(_userManager.GetUserId(User));

        try
        {
            var cartItems = await _getUserCartUseCase.Execute(userId);
            return Ok(cartItems);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error for user {UserId}", userId);
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart items for user {UserId}", userId);
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpGet("info")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> GetCartItemsInfo([FromQuery] Guid[] variantIds)
    {
        _logger.LogInformation("Request received for variant IDs: {VariantIds}", variantIds);

        if (variantIds == null || variantIds.Length == 0)
        {
            _logger.LogWarning("Empty variant IDs array received");
            return BadRequest(new { Error = "At least one variant ID must be provided" });
        }

        try
        {
            var itemsInfo = await _getCartItemsInfoUseCase.Execute(variantIds);
            _logger.LogInformation("Returning info for {Count} cart items", itemsInfo.Count());
            return Ok(itemsInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing cart items request");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    [HttpPost("remove")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> RemoveFromCart([FromBody] RemoveFromCartRequest request)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User));

        try
        {
            var userRoleInfo = await _getUserRoleUseCase.Execute(userId);

            if (!userRoleInfo.CanPurchase)
            {
                return userRoleInfo.Role == Role.seller
                    ? StatusCode(403, new { Error = "Продавцы должны включить функциональность покупателя, чтобы удалять товары из корзины" })
                    : StatusCode(403, new { Error = "Только клиенты могут удалять товары из корзины" });
            }

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