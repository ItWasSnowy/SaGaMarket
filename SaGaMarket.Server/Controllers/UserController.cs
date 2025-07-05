using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Services;
using SaGaMarket.Core.UseCases.UserUseCases;
using SaGaMarket.Server.Identity;
using System;
using System.Threading.Tasks;

namespace SaGaMarket.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Требует аутентификации для всех endpoints
    public class UserController : ControllerBase
    {
        private readonly CreateUserUseCase _createUserUseCase;
        private readonly GetUserUseCase _getUserUseCase;
        private readonly UpdateUserUseCase _updateUserUseCase;
        private readonly DeleteUserUseCase _deleteUserUseCase;
        private readonly GetUserRoleUseCase _getUserRoleUseCase;
        private readonly UserManager<SaGaMarketIdentityUser> _userManager;

        public UserController(
            CreateUserUseCase createUserUseCase,
            GetUserUseCase getUserUseCase,
            UpdateUserUseCase updateUserUseCase,
            DeleteUserUseCase deleteUserUseCase,
            GetUserRoleUseCase getUserRoleUseCase,
            UserManager<SaGaMarketIdentityUser> userManager)
        {
            _createUserUseCase = createUserUseCase;
            _getUserUseCase = getUserUseCase;
            _updateUserUseCase = updateUserUseCase;
            _deleteUserUseCase = deleteUserUseCase;
            _getUserRoleUseCase = getUserRoleUseCase;
            _userManager = userManager;
        }

        [HttpPost]
        [AllowAnonymous] // Разрешает доступ без аутентификации
        public async Task<IActionResult> Create([FromBody] UserDto userDto)
        {
            if (userDto == null)
            {
                return BadRequest("Invalid user data.");
            }

            var userId = await _createUserUseCase.Handle(userDto);
            return CreatedAtAction(nameof(Get), new { id = userId }, null);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            // Проверка, что пользователь запрашивает свои данные или является админом
            var currentUserId = Guid.Parse(_userManager.GetUserId(User));
            if (id != currentUserId && !User.IsInRole("admin"))
            {
                return Forbid();
            }

            var user = await _getUserUseCase.Handle(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserDto userDto)
        {
            if (userDto == null)
            {
                return BadRequest("Invalid user data.");
            }

            // Проверка прав доступа
            var currentUserId = Guid.Parse(_userManager.GetUserId(User));
            if (id != currentUserId && !User.IsInRole("admin"))
            {
                return Forbid();
            }

            userDto.UserId = id;

            try
            {
                await _updateUserUseCase.Handle(userDto);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the user.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // Только админ может удалять пользователей
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // Получаем текущего пользователя из Identity
                var currentUserId = Guid.Parse(_userManager.GetUserId(User));
                await _deleteUserUseCase.Handle(id, currentUserId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the user.");
            }
        }

        [HttpGet("{userId}/role")]
        [ProducesResponseType(typeof(UserRoleInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserRole(Guid userId)
        {
            // Проверка прав доступа
            var currentUserId = Guid.Parse(_userManager.GetUserId(User));
            if (userId != currentUserId && !User.IsInRole("admin"))
            {
                return Forbid();
            }

            try
            {
                var roleInfo = await _getUserRoleUseCase.Execute(userId);

                return Ok(new
                {
                    UserId = userId,
                    Role = roleInfo.Role.ToString(),
                    roleInfo.CanPurchase,
                    roleInfo.CanSell,
                    roleInfo.IsAdmin,
                    roleInfo.RoleDescription
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var user = await _getUserUseCase.Handle(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Добавляем информацию из Identity
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            var roles = await _userManager.GetRolesAsync(identityUser);

            return Ok(new
            {
                user.UserId,
                user.Role,
                identityUser.Email,
                identityUser.UserName,
                identityUser.PhoneNumber,
                Roles = roles,
                identityUser.ProfilePhotoUrl
            });
        }
    }
}