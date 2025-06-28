using Microsoft.AspNetCore.Mvc;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Services;
using SaGaMarket.Core.UseCases.UserUseCases;
using System;
using System.Threading.Tasks;

namespace SaGaMarket.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly CreateUserUseCase _createUserUseCase;
        private readonly GetUserUseCase _getUserUseCase;
        private readonly UpdateUserUseCase _updateUserUseCase;
        private readonly DeleteUserUseCase _deleteUserUseCase;
        private readonly GetUserRoleUseCase _getUserRoleUseCase;

        public UserController(
            CreateUserUseCase createUserUseCase,
            GetUserUseCase getUserUseCase,
            UpdateUserUseCase updateUserUseCase,
            DeleteUserUseCase deleteUserUseCase,
            GetUserRoleUseCase getUserRoleUseCase)
        {
            _createUserUseCase = createUserUseCase;
            _getUserUseCase = getUserUseCase;
            _updateUserUseCase = updateUserUseCase;
            _deleteUserUseCase = deleteUserUseCase;
            _getUserRoleUseCase = getUserRoleUseCase;
            _getUserRoleUseCase = getUserRoleUseCase;
        }

        [HttpPost]
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

            userDto.UserId = id; // Убедитесь, что ID пользователя совпадает с переданным в URL

            try
            {
                await _updateUserUseCase.Handle(userDto);
                return NoContent(); // Успешное обновление
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the user.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid currentUserId)
        {
            try
            {
                await _deleteUserUseCase.Handle(id, currentUserId);
                return NoContent(); // Успешное удаление
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
    }

}
