using System.Threading.Tasks;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases
{
    public class GetCommentsByReviewUseCase
    {
        private readonly ICommentRepository _commentRepository;
        public GetCommentsByReviewUseCase(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }
        public async Task<List<CommentDto>?> Handle(Guid reviewId)
        {
            var comments = await _commentRepository.GetByReview(reviewId);
            if (comments == null || !comments.Any()) return null;

            var commentDtos = comments.Select(comment => new CommentDto(comment)).ToList();

            return commentDtos;
        }
    }
}
