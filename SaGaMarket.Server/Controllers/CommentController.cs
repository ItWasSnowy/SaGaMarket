using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.UseCases;
using SaGaMarket.Core.UseCases.CommentUseCases;
using SaGaMarket.Identity;
using SaGaMarket.Server.Identity;
using System;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CommentController : ControllerBase
{
    private readonly CreateCommentUseCase _createCommentUseCase;
    private readonly DeleteCommentUseCase _deleteCommentUseCase;
    private readonly GetCommentUseCase _getCommentUseCase;
    private readonly GetCommentsByAuthorUseCase _getCommentsByAuthorUseCase;
    private readonly GetCommentsByReviewUseCase _getCommentsByReviewUseCase;
    private readonly UpdateCommentUseCase _updateCommentUseCase;
    private readonly UserManager<SaGaMarketIdentityUser> _userManager;

    public CommentController(
        CreateCommentUseCase createCommentUseCase,
        DeleteCommentUseCase deleteCommentUseCase,
        GetCommentUseCase getCommentUseCase,
        GetCommentsByAuthorUseCase getCommentsByAuthorUseCase,
        GetCommentsByReviewUseCase getCommentsByReviewUseCase,
        UpdateCommentUseCase updateCommentUseCase,
        UserManager<SaGaMarketIdentityUser> userManager)
    {
        _createCommentUseCase = createCommentUseCase;
        _deleteCommentUseCase = deleteCommentUseCase;
        _getCommentUseCase = getCommentUseCase;
        _getCommentsByAuthorUseCase = getCommentsByAuthorUseCase;
        _getCommentsByReviewUseCase = getCommentsByReviewUseCase;
        _updateCommentUseCase = updateCommentUseCase;
        _userManager = userManager;
    }

    [HttpPost]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
    {
        try
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            request.AuthorId = userId;

            var commentId = await _createCommentUseCase.Handle(request);
            return Ok(new { CommentId = commentId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpDelete("delete/{commentId}")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<ActionResult> DeleteComment(Guid commentId)
    {
        try
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _deleteCommentUseCase.Handle(commentId, userId);
            return Ok(new { Message = "Комментарий успешно удален" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("get/{commentId}")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<ActionResult<CommentDto>> GetComment(Guid commentId)
    {
        var result = await _getCommentUseCase.Handle(commentId);
        return result == null ? NotFound(new { Error = "Комментарий не найден" }) : Ok(result);
    }

    [HttpGet("by-author/{authorId}")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<ActionResult> GetCommentsByAuthor(Guid authorId)
    {
        var currentUserId = Guid.Parse(_userManager.GetUserId(User));
        if (authorId != currentUserId && !User.IsInRole("admin"))
        {
            return Forbid();
        }

        var result = await _getCommentsByAuthorUseCase.Handle(authorId);
        return result == null ? NotFound(new { Error = "Комментарии не найдены" }) : Ok(result);
    }

    [HttpGet("by-review/{reviewId}")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<ActionResult> GetCommentsByReview(Guid reviewId)
    {
        var result = await _getCommentsByReviewUseCase.Handle(reviewId);
        return result == null ? NotFound(new { Error = "Комментарии не найдены" }) : Ok(result);
    }

    [HttpPut("update")]
    [Authorize(Roles = "customer,seller,admin")]
    public async Task<ActionResult> UpdateComment([FromBody] UpdateCommentRequest request)
    {
        try
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            await _updateCommentUseCase.Handle(request.CommentId, request.NewCommentText, userId);
            return Ok(new { Message = "Комментарий успешно обновлен" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    public class UpdateCommentRequest
    {
        public Guid CommentId { get; set; }
        public string NewCommentText { get; set; } = string.Empty;
    }
}