using System.Threading.Tasks;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases
{
    public class GetCommentsByAuthorUseCase
    {
        private readonly ICommentRepository _commentRepository;
        public GetCommentsByAuthorUseCase(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }
        public async Task<List<CommentDto>?> Handle(Guid authorId)
        {
            
            // Получаем список комментариев от автора
            var comments = await _commentRepository.GetByAuthor(authorId);
            if (comments == null || !comments.Any()) return null;

            var commentDtos = comments.Select(comment => new CommentDto(comment)).ToList();

            return commentDtos;
        }
    }
}