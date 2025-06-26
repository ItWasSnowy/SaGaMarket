using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Dtos;

public class CommentDto
{
    public Guid CommentId { get; set; }
    public Guid AuthorId { get; set; } // Внешний ключ
    public Guid ReviewId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public DateTime TimeCreate { get; set; }
    public DateTime TimeLastUpdate { get; set; }

    public CommentDto()
    {

    }
    public CommentDto(Comment comment)
    {
        CommentId = comment.CommentId;
        CommentText = comment.CommentText;
        TimeCreate = comment.TimeCreate;
        TimeLastUpdate = comment.TimeLastUpdate;
    }
}
