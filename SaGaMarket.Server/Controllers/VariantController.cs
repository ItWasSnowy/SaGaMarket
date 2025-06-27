using Microsoft.AspNetCore.Mvc;
using SaGaMarket.Core.UseCases.VariantUseCases;
using System;
using System.Threading.Tasks;

namespace SaGaMarket.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VariantController : ControllerBase
    {
        private readonly CreateVariantUseCase _createVariantUseCase;
        private readonly GetVariantUseCase _getVariantUseCase;
        private readonly UpdateVariantUseCase _updateVariantUseCase;
        private readonly DeleteVariantUseCase _deleteVariantUseCase;

        public VariantController(
            CreateVariantUseCase createVariantUseCase,
            GetVariantUseCase getVariantUseCase,
            UpdateVariantUseCase updateVariantUseCase,
            DeleteVariantUseCase deleteVariantUseCase)
        {
            _createVariantUseCase = createVariantUseCase;
            _getVariantUseCase = getVariantUseCase;
            _updateVariantUseCase = updateVariantUseCase;
            _deleteVariantUseCase = deleteVariantUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVariantUseCase.CreateVariantRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid variant data.");
            }
            var variantId = await _createVariantUseCase.Handle(request);
            return CreatedAtAction(nameof(Get), new { id = variantId }, null);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var variant = await _getVariantUseCase.Handle(id);
            if (variant == null)
            {
                return NotFound();
            }

            return Ok(variant);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVariantUseCase.UpdateVariantRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid variant data.");
            }

            try
            {
                await _updateVariantUseCase.Handle(id, request);
                return NoContent(); // Успешное обновление
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the variant.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _deleteVariantUseCase.Handle(id);
                return NoContent(); // Успешное удаление
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the variant.");
            }
        }
    }
}
