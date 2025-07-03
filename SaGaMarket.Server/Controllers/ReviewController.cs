using Microsoft.AspNetCore.Mvc;
using SaGaMarket.Core.UseCases.ReviewUseCases;
using System;
using System.Threading.Tasks;

namespace SaGaMarket.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly CreateReviewUseCase _createReviewUseCase;
        private readonly GetReviewUseCase _getReviewUseCase;
        private readonly UpdateReviewUseCase _updateReviewUseCase;
        private readonly DeleteReviewUseCase _deleteReviewUseCase;
        private readonly GetAllReviewsOfOneProductUseCase _getAllReviewsOfOneProductUseCase;

        public ReviewController(
            CreateReviewUseCase createReviewUseCase,
            GetReviewUseCase getReviewUseCase,
            UpdateReviewUseCase updateReviewUseCase,
            GetAllReviewsOfOneProductUseCase getAllReviewsOfOneProductUseCase,
            DeleteReviewUseCase deleteReviewUseCase)
        {
            _createReviewUseCase = createReviewUseCase;
            _getAllReviewsOfOneProductUseCase = getAllReviewsOfOneProductUseCase;
            _getReviewUseCase = getReviewUseCase;
            _updateReviewUseCase = updateReviewUseCase;
            _deleteReviewUseCase = deleteReviewUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReviewUseCase.CreateReviewRequest request, [FromQuery] Guid authorId)
        {
            if (request == null)
            {
                return BadRequest("Invalid review data.");
            }

            try
            {
                var reviewId = await _createReviewUseCase.Handle(request, authorId);
                return CreatedAtAction(nameof(Get), new { id = reviewId }, null);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message); 
            }
            
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var review = await _getReviewUseCase.Handle(id);
            if (review == null)
            {
                return NotFound();
            }

            return Ok(review);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] double newRating, [FromQuery] Guid authorId)
        {
            try
            {
                await _updateReviewUseCase.Handle(id, newRating, authorId);
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
                return StatusCode(500, "An error occurred while updating the review.");
            }
        }

        [HttpGet("/api/products/{productId}/reviews")]
        public async Task<IActionResult> GetAllProductReviews(Guid productId)
        {
            try
            {
                var reviews = await _getAllReviewsOfOneProductUseCase.Handle(productId);
                return Ok(reviews);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message); // 404 если продукт не найден
            }
            catch (Exception)
            {
                return StatusCode(500, "Внутренняя ошибка сервера"); // 500 для других ошибок
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid authorId)
        {
            try
            {
                await _deleteReviewUseCase.Handle(id, authorId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the review.");
            }
        }
    }
}
