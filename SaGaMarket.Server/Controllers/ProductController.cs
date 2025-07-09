using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.UseCases.ProductUseCases;
using SaGaMarket.Core.UseCases.ReviewUseCases;
using SaGaMarket.Identity;
using SaGaMarket.Server.Identity;
using SaGaMarket.Storage.EfCore;
using SaGaMarket.Storage.EfCore.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;
using static CreateProductUseCase;
using static SaGaMarket.Core.UseCases.ProductUseCases.UpdateProductUseCase;

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
        private readonly UserManager<SaGaMarketIdentityUser> _userManager;
        private readonly SaGaMarketDbContext _context;
        private readonly ILogger<ProductController> _logger;
        private readonly ProductRepository _productRepository;

        public ProductController(
            ProductRepository productRepository,
            SaGaMarketDbContext context,
            CreateProductUseCase createProductUseCase,
            GetProductUseCase getProductUseCase,
            UpdateProductUseCase updateProductUseCase,
            GetProductWithPagination getProductWithPagination,
            GetProductsRatingsUseCase getProductsRatingsUseCase,
            DeleteProductUseCase deleteProductUseCase,
            UserManager<SaGaMarketIdentityUser> userManager,
            ILogger<ProductController> logger)
        {
            _productRepository = productRepository;
            _context = context;
            _createProductUseCase = createProductUseCase;
            _getProductUseCase = getProductUseCase;
            _getProductWithPagination = getProductWithPagination;
            _updateProductUseCase = updateProductUseCase;
            _deleteProductUseCase = deleteProductUseCase;
            _getProductsRatingsUseCase = getProductsRatingsUseCase;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var (products, totalCount) = await _getProductWithPagination
                    .GetProductsWithPaginationAsync(page, pageSize);

                Response.Headers.Append("X-Total-Count", totalCount.ToString());
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, new
                {
                    Error = "Internal server error",
                    Details = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "seller,admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            try
            {
                var sellerId = Guid.Parse(_userManager.GetUserId(User));
                var productId = await _createProductUseCase.Handle(request, sellerId);

                return CreatedAtAction(nameof(Get), new { id = productId }, new
                {
                    ProductId = productId,
                    Message = "Product created successfully"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid product data");
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var product = await _getProductUseCase.Handle(id);
                return product == null
                    ? NotFound(new { Error = "Product not found" })
                    : Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId}", id);
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        //[HttpGet("categories")]
        //public async Task<IActionResult> GetCategories()
        //{
        //    try
        //    {
        //        var categories = await _productRepository.GetCategories();
        //        return Ok(categories);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting categories");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        [HttpGet("filtered")]
        public async Task<IActionResult> GetFilteredProducts(
    [FromQuery] string? category,
    [FromQuery] double? minRating,
    [FromQuery] string? searchTerm,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 12)
        {
            try
            {
                var products = await _productRepository.GetFilteredProducts(
                    category, minRating, searchTerm, page, pageSize);

                var totalCount = await _productRepository.GetFilteredProductsCount(
                    category, minRating, searchTerm);

                Response.Headers.Append("X-Total-Count", totalCount.ToString());

                // Включаем связанные данные (варианты и отзывы)
                var productsWithDetails = await _context.Products
                    .Include(p => p.Variants)
                    .Include(p => p.Reviews)
                    .Where(p => products.Select(prod => prod.ProductId).Contains(p.ProductId))
                    .ToListAsync();

                return Ok(productsWithDetails.Select(p => new ProductDto(p)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filtered products");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "seller,admin")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateProductRequest request)
        {
            try
            {
                var sellerId = Guid.Parse(_userManager.GetUserId(User));
                await _updateProductUseCase.Handle(id, request, sellerId);

                return Ok(new { Message = "Product updated successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Product not found {ProductId}", id);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Unauthorized access to product {ProductId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }


        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _productRepository.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("ratings")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsRatings([FromQuery] string productIds)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productIds))
                {
                    return BadRequest(new { Error = "Product IDs are required" });
                }

                var ids = productIds.Split(',')
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(Guid.Parse)
                    .ToList();

                var ratings = await _getProductsRatingsUseCase.Handle(ids);
                return Ok(ratings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product ratings");
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "seller,admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var sellerId = Guid.Parse(_userManager.GetUserId(User));
                await _deleteProductUseCase.Handle(id, sellerId);

                return Ok(new { Message = "Product deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Product not found {ProductId}", id);
                return NotFound(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Unauthorized delete attempt {ProductId}", id);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, new { Error = "Internal server error" });
            }
        }
        // Пример для ASP.NET Core
        [HttpGet("api/Products")]
        public IActionResult GetProductsBySeller([FromQuery] string sellerId)
        {
            var products = _context.Products
                .Where(p => p.SellerId.ToString() == sellerId)
                .ToList();

            return Ok(products);
        }
    }
}