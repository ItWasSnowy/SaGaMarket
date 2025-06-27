using SaGaMarket.Core.Entities;
using SaGaMarket.Core.Storage.Repositories;
using System.ComponentModel.DataAnnotations;

public class CreateCommentUseCase
{
    private readonly ICommentRepository _commentRepository;

    public CreateCommentUseCase(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Guid> Handle(CreateCommentRequest request)
    {
        var comment = new Comment
        {
            ReviewId = request.ReviewId,
            CommentText = request.CommentText,
            AuthorId = request.AuthorId,
            TimeCreate = DateTime.UtcNow,
            TimeLastUpdate = DateTime.UtcNow
        };

        return await _commentRepository.Create(comment);
    }
}

public class CreateCommentRequest
{
    [Required]
    public Guid AuthorId { get; set; }

    [Required]
    public Guid ReviewId { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string CommentText { get; set; } = string.Empty;
}