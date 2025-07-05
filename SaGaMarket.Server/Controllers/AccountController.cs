using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SaGaMarket.Identity;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Dtos;
using Microsoft.AspNetCore.Authorization;
using SaGaMarket.Server.Identity;
using Microsoft.EntityFrameworkCore;
using SaGaMarket.Storage.EfCore;

namespace SaGaMarket.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<SaGaMarketIdentityUser> _userManager;
    private readonly SignInManager<SaGaMarketIdentityUser> _signInManager;
    private readonly SaGaMarketDbContext _context;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public AccountController(
        UserManager<SaGaMarketIdentityUser> userManager,
        SignInManager<SaGaMarketIdentityUser> signInManager,
        SaGaMarketDbContext context,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _roleManager = roleManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Проверка роли
        if (!Enum.TryParse<Role>(request.Role, true, out var role) ||
            !Enum.IsDefined(typeof(Role), role))
        {
            return BadRequest("Invalid role specified");
        }

        var user = new SaGaMarketIdentityUser
        {
            UserName = request.Email,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // Создаем роль, если ее нет
        if (!await _roleManager.RoleExistsAsync(role.ToString()))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>(role.ToString()));
        }

        // Назначаем роль пользователю
        await _userManager.AddToRoleAsync(user, role.ToString());

        // Создаем связанную запись в основной БД
        var newUser = new User
        {
            UserId = user.Id,
            Role = role
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        // Автоматический вход после регистрации
        await _signInManager.SignInAsync(user, isPersistent: false);

        return Ok(new
        {
            UserId = user.Id,
            Email = user.Email,
            Role = role.ToString()
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized("Invalid login attempt");

        var result = await _signInManager.PasswordSignInAsync(
            user, request.Password, request.RememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
            return Unauthorized("Invalid login attempt");

        // Получаем роли пользователя
        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = roles,
            ProfilePhotoUrl = user.ProfilePhotoUrl
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { Message = "Logout successful" });
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found");

        var dbUser = await _context.Users
            .Include(u => u.ProductsForSale)
            .Include(u => u.Orders)
            .FirstOrDefaultAsync(u => u.UserId == user.Id);

        if (dbUser == null)
            return NotFound("User data not found");

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new UserProfileDto
        {
            UserId = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            Role = dbUser.Role.ToString(),
            ProductsForSaleCount = dbUser.ProductsForSale?.Count ?? 0,
            OrdersCount = dbUser.Orders?.Count ?? 0,
            CreatedAt = user.CreatedAt
        });
    }

    [Authorize(Roles = "admin")]
    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            return NotFound("User not found");

        if (!Enum.TryParse<Role>(request.Role, true, out var role) ||
            !Enum.IsDefined(typeof(Role), role))
        {
            return BadRequest("Invalid role specified");
        }

        // Удаляем все текущие роли
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Добавляем новую роль
        await _userManager.AddToRoleAsync(user, role.ToString());

        // Обновляем роль в основной БД
        var dbUser = await _context.Users.FindAsync(user.Id);
        if (dbUser != null)
        {
            dbUser.Role = role;
            await _context.SaveChangesAsync();
        }

        return Ok(new { Message = $"Role {role} assigned successfully" });
    }
}

public class RegisterRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; } // "customer", "seller" или "admin"
}

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public bool RememberMe { get; set; } = false;
}

public class AssignRoleRequest
{
    public Guid UserId { get; set; }
    public required string Role { get; set; }
}

public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string ProfilePhotoUrl { get; set; }
    public string Role { get; set; }
    public int ProductsForSaleCount { get; set; }
    public int OrdersCount { get; set; }
    public DateTime CreatedAt { get; set; }
}