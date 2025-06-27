using SaGaMarket.Core.Dtos;
using System.ComponentModel.DataAnnotations;

namespace SaGaMarket.Core.Entities;

public class Comment
{
    public Guid CommentId { get; set; }

    [Required]
    public Guid AuthorId { get; set; }

    [Required]
    public Guid ReviewId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string CommentText { get; set; } = string.Empty;

    public DateTime TimeCreate { get; set; } = DateTime.UtcNow;
    public DateTime TimeLastUpdate { get; set; } = DateTime.UtcNow;

    // Навигационные свойства
    public User Author { get; set; } = null!;
    public Review Review { get; set; } = null!;
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


