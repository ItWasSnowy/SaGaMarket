using System.Threading.Tasks;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases
{
    public class UpdateCommentUseCase
    {
        private readonly ICommentRepository _commentRepository;

        public UpdateCommentUseCase(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task Handle(Guid commentId, string newCommentText, Guid currentUserId)
        {
            if (string.IsNullOrWhiteSpace(newCommentText))
                throw new ArgumentException("Comment body cannot be empty");

            var existingComment = await _commentRepository.Get(commentId);
            if (existingComment == null)
                throw new ArgumentException("Comment not found");

            if (existingComment.AuthorId != currentUserId)
                throw new UnauthorizedAccessException("You can only update your own comments");

            var updatedComment = new Comment
            {
                CommentId = existingComment.CommentId,
                CommentText = newCommentText,
                AuthorId = existingComment.AuthorId,
                ReviewId = existingComment.ReviewId,
                TimeLastUpdate = DateTime.UtcNow,
              
            };

            if (!await _commentRepository.Update(updatedComment))
                throw new Exception("Failed to update comment");
        }
    }
}