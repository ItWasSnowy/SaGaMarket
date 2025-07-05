using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SaGaMarket.Core.UseCases.ReviewUseCases;
using SaGaMarket.Identity;
using SaGaMarket.Server.Identity;
using System;
using System.Threading.Tasks;
using static SaGaMarket.Core.UseCases.ReviewUseCases.CreateReviewUseCase;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly CreateReviewUseCase _createReviewUseCase;
    private readonly GetReviewUseCase _getReviewUseCase;
    private readonly UpdateReviewUseCase _updateReviewUseCase;
    private readonly DeleteReviewUseCase _deleteReviewUseCase;
    private readonly GetAllReviewsOfOneProductUseCase _getAllReviewsOfOneProductUseCase;
    private readonly UserManager<SaGaMarketIdentityUser> _userManager;

    public ReviewController(
        CreateReviewUseCase createReviewUseCase,
        GetReviewUseCase getReviewUseCase,
        UpdateReviewUseCase updateReviewUseCase,
        GetAllReviewsOfOneProductUseCase getAllReviewsOfOneProductUseCase,
        DeleteReviewUseCase deleteReviewUseCase,
        UserManager<SaGaMarketIdentityUser> userManager)
    {
        _createReviewUseCase = createReviewUseCase;
        _getAllReviewsOfOneProductUseCase = getAllReviewsOfOneProductUseCase;
        _getReviewUseCase = getReviewUseCase;
        _updateReviewUseCase = updateReviewUseCase;
        _deleteReviewUseCase = deleteReviewUseCase;
        _userManager = userManager;
    }

    [HttpPost]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest request)
    {
        try
        {
            var authorId = Guid.Parse(_userManager.GetUserId(User));
            var reviewId = await _createReviewUseCase.Handle(request, authorId);
            return Ok(new { ReviewId = reviewId });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            var review = await _getReviewUseCase.Handle(id);
            return review == null
                ? NotFound(new { Error = "Review not found" })
                : Ok(review);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] double newRating)
    {
        try
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _updateReviewUseCase.Handle(id, newRating, userId);
            return Ok(new { Message = "Review updated successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpGet("products/{productId}/reviews")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllProductReviews(Guid productId)
    {
        try
        {
            var reviews = await _getAllReviewsOfOneProductUseCase.Handle(productId);
            return Ok(reviews);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _deleteReviewUseCase.Handle(id, userId);
            return Ok(new { Message = "Review deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }
}