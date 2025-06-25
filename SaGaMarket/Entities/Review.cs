using SaGaMarket.Core.Dtos;

namespace SaGaMarket.Core.Entities;

public class Review
{
    public Guid ReviewId { get; set; }
    public Guid ProductId { get; set; }
    public double UserRating { get; set; }
    public List<Guid> CommentIds { get; set; } = [];
    public Guid AuthorId { get; set; } // Внешний ключ


    // Navigation properties
    public User Author { get; set; }
    public Product Product { get; set; }
    public List<Comment> Comments { get; set; } = new();


    public Review() { }
    public Review(ReviewDto reviewDto) 
    {
        ReviewId = reviewDto.ReviewId;
        UserRating = reviewDto.UserRating;
        CommentIds = reviewDto.CommentIds;
    }

}
