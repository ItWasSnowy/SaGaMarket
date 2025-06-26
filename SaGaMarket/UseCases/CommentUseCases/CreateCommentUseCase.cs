using SaGaMarket.Core.Dtos;
using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;


namespace SaGaMarket.Core.UseCases.CommentUseCases;

public class CreateCommentUseCase
{
   
    private readonly ICommentRepository _commentRepository;

    public CreateCommentUseCase(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Guid> Handle(CreateCommentRequest request, Guid currentUserId)
    {
        var comment = new Comment()
        {
            ReviewId = request.ReviewId,
            CommentText = request.CommentText,
            AuthorId = currentUserId,
            TimeLastUpdate = DateTime.UtcNow,
        };
        return await _commentRepository.Create(comment);
    }

    public class CreateCommentRequest
    {
        public Guid ReviewId { get; set; }
        public string CommentText { get; set; } = string.Empty;
    }

}

