using System;
using System.Threading.Tasks;
using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;

namespace SaGaMarket.Core.UseCases.CommentUseCases
{
    public class GetCommentUseCase
    {
        private readonly ICommentRepository _commentRepository;
        public GetCommentUseCase(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }
        public async Task<CommentDto?> Handle(Guid commentId)
        {
            var comment = await _commentRepository.Get(commentId);
            if (comment == null) return null ;           

            return new CommentDto(comment);
        }

    }
}