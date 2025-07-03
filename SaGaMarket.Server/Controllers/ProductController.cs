using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SaGaMarket.Core.UseCases.ProductUseCases;
using SaGaMarket.Core.UseCases.ReviewUseCases;
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
        private readonly GetProductWithPagination _getProductWithPagination;
        private readonly GetProductsRatingsUseCase _getProductsRatingsUseCase;

        public ProductController(
            CreateProductUseCase createProductUseCase,
            GetProductUseCase getProductUseCase,
            UpdateProductUseCase updateProductUseCase,
            GetProductWithPagination getProductWithPagination,
            GetProductsRatingsUseCase getProductsRatingsUseCase,
        DeleteProductUseCase deleteProductUseCase)
        {
            _createProductUseCase = createProductUseCase;
            _getProductUseCase = getProductUseCase;
            _getProductWithPagination = getProductWithPagination;
            _updateProductUseCase = updateProductUseCase;
            _deleteProductUseCase = deleteProductUseCase;
            _getProductsRatingsUseCase = getProductsRatingsUseCase;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
        {
            try
            {
                var (products, totalCount) = await _getProductWithPagination
                    .GetProductsWithPaginationAsync(page, pageSize);

                HttpContext.Response.Headers.Append("X-Total-Count", totalCount.ToString());

                return Ok(products);
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    Message = "Ошибка сервера",
                    Detailed = ex.Message
                });
            }
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
                return NoContent();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the product.");
            }
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProductsRatings([FromQuery] string productIds)
        {
            try
            {
                var ids = productIds.Split(',')
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(Guid.Parse)
                .ToList();

                var ratings = await _getProductsRatingsUseCase.Handle(ids);
                return Ok(ratings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid sellerId)
        {
            try
            {
                await _deleteProductUseCase.Handle(id, sellerId);
                return NoContent();
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
