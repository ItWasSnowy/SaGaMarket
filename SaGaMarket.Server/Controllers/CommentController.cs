using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.UseCases;
using SaGaMarket.Core.UseCases.CommentUseCases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace SaGaMarket.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly CreateCommentUseCase _createCommentUseCase;
        private readonly DeleteCommentUseCase _deleteCommentUseCase;
        private readonly GetCommentUseCase _getCommentUseCase;
        private readonly GetCommentsByAuthorUseCase _getCommentsByAuthorUseCase;
        private readonly GetCommentsByReviewUseCase _getCommentsByReviewUseCase;
        private readonly UpdateCommentUseCase _updateCommentUseCase;

        public CommentController(
            CreateCommentUseCase createCommentUseCase,
            DeleteCommentUseCase deleteCommentUseCase,
            GetCommentUseCase getCommentUseCase,
            GetCommentsByAuthorUseCase getCommentsByAuthorUseCase,
            GetCommentsByReviewUseCase getCommentsByReviewUseCase,
            UpdateCommentUseCase updateCommentUseCase)
        {
            _createCommentUseCase = createCommentUseCase;
            _deleteCommentUseCase = deleteCommentUseCase;
            _getCommentUseCase = getCommentUseCase;
            _getCommentsByAuthorUseCase = getCommentsByAuthorUseCase;
            _getCommentsByReviewUseCase = getCommentsByReviewUseCase;
            _updateCommentUseCase = updateCommentUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
        {
            try
            {
                var commentId = await _createCommentUseCase.Handle(request);
                return Ok(commentId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete/{commentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteComment(Guid commentId)
        {
            try
            {
                await _deleteCommentUseCase.Handle(commentId/*, Guid.NewGuid()*/); 
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("get/{commentId}")]
        [ProducesResponseType<CommentDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommentDto?>> GetComment(Guid commentId)
        {
            var result = await _getCommentUseCase.Handle(commentId);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("by-author/{authorId}")]
        [ProducesResponseType<List<CommentDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<CommentDto>>> GetCommentsByAuthor(Guid authorId)
        {
            var result = await _getCommentsByAuthorUseCase.Handle(authorId);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("by-review/{reviewId}")]
        [ProducesResponseType<List<CommentDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<CommentDto>>> GetCommentsByReview(Guid reviewId)
        {
            var result = await _getCommentsByReviewUseCase.Handle(reviewId);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPut("update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateComment([FromBody] UpdateCommentRequest request)
        {
            try
            {
                await _updateCommentUseCase.Handle(request.CommentId, request.NewCommentText);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        public class UpdateCommentRequest
        {
            public Guid CommentId { get; set; }
            public string NewCommentText { get; set; } = string.Empty;
        }
    }
}
