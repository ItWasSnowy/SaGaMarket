using SaGaMarket.Core.Dtos;

namespace SaGaMarket.Core.Entities;

public class Comment
{
    public Guid CommentId { get; set; }
    public Guid AuthorId { get; set; } // Внешний ключ
    public Guid ReviewId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public DateTime TimeCreate { get; set; }
    public DateTime TimeLastUpdate { get; set; }

    // Navigation properties
    public User Author { get; set; }
    public Review Review { get; set; }

    public Comment()
    {

    }
    public Comment(CommentDto commentDto)
    {
        CommentId = commentDto.CommentId;
        CommentText = commentDto.CommentText;
        TimeCreate = commentDto.TimeCreate;
        TimeLastUpdate = commentDto.TimeLastUpdate;
    }
}
