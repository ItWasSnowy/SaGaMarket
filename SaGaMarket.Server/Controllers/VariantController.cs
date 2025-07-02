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
        private readonly GetAllVariantsOfOneProductUseCase _getAllVariantsUseCase;
        private readonly UpdateCountVariantUseCase _updateCountVariantUseCase;

        public VariantController(
            CreateVariantUseCase createVariantUseCase,
            GetVariantUseCase getVariantUseCase,
            UpdateVariantUseCase updateVariantUseCase,
            GetAllVariantsOfOneProductUseCase getAllVariantsUseCase,
            UpdateCountVariantUseCase updateCountVariantUseCase,
            DeleteVariantUseCase deleteVariantUseCase)
        {
            _createVariantUseCase = createVariantUseCase;
            _getVariantUseCase = getVariantUseCase;
            _updateVariantUseCase = updateVariantUseCase;
            _deleteVariantUseCase = deleteVariantUseCase;
            _updateCountVariantUseCase = updateCountVariantUseCase;
            _getAllVariantsUseCase = getAllVariantsUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVariantUseCase.CreateVariantRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid variant data.");
            }
            try
            {
                var variantId = await _createVariantUseCase.Handle(request);
                return CreatedAtAction(nameof(Get), new { id = variantId }, null);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while creating the variant.");
            }
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

        [HttpGet("/api/products/{productId}/variants")]
        public async Task<IActionResult> GetAllVariants(Guid productId)
        {
            try
            {
                var variants = await _getAllVariantsUseCase.Handle(productId);
                return Ok(variants);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
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
                return NoContent();
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

        [HttpPut]
        public async Task<IActionResult> UpdateCount(Guid id, [FromBody] UpdateCountVariantUseCase.UpdateCountVariantRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid variant data.");
            }

            try
            {
                await _updateCountVariantUseCase.Handle(id, request);
                return NoContent();
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
                return NoContent();
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
