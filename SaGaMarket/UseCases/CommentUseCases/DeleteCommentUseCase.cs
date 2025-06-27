using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.CommentUseCases
{
    public class DeleteCommentUseCase
    {
        private readonly ICommentRepository _commentRepository;
        public DeleteCommentUseCase(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }
        public async Task Handle(Guid commentId/*, Guid currentUserId*/)
        {
            var c = await _commentRepository.Get(commentId);
            if (c is null) throw new InvalidOperationException("Comment not found");
            //if (c.AuthorId != currentUserId) throw new InvalidOperationException("Is not the author");
            await _commentRepository.Delete(commentId);
        }
    }
}