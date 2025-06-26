using Microsoft.AspNetCore.Mvc;
using SaGaMarket.Core.UseCases.ProductUseCases;
using System;
using System.Threading.Tasks;

namespace SaGaMarket.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly CreateProductUseCase _createProductUseCase;
        private readonly GetProductUseCase _getProductUseCase;
        private readonly UpdateProductUseCase _updateProductUseCase;
        private readonly DeleteProductUseCase _deleteProductUseCase;

        public ProductController(
            CreateProductUseCase createProductUseCase,
            GetProductUseCase getProductUseCase,
            UpdateProductUseCase updateProductUseCase,
            DeleteProductUseCase deleteProductUseCase)
        {
            _createProductUseCase = createProductUseCase;
            _getProductUseCase = getProductUseCase;
            _updateProductUseCase = updateProductUseCase;
            _deleteProductUseCase = deleteProductUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductUseCase.CreateProductRequest request, [FromQuery] Guid sellerId)
        {
            if (request == null)
            {
                return BadRequest("Invalid product data.");
            }

            var productId = await _createProductUseCase.Handle(request, sellerId);
            return CreatedAtAction(nameof(Get), new { id = productId }, null);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var product = await _getProductUseCase.Handle(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductUseCase.UpdateProductRequest request, [FromQuery] Guid sellerId)
        {
            try
            {
                await _updateProductUseCase.Handle(id, request, sellerId);
                return NoContent(); // Успешное обновление
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid(); // Доступ запрещен
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the product.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid sellerId)
        {
            try
            {
                await _deleteProductUseCase.Handle(id, sellerId);
                return NoContent(); // Успешное удаление
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the product.");
            }
        }
    }
}
