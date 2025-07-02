using SaGaMarket.Core.Dtos;

namespace SaGaMarket.Core.Entities;

public class Review
{
    public Guid ReviewId { get; set; }
    public Guid ProductId { get; set; }
    public double UserRating { get; set; }
    public List<Guid> CommentIds { get; set; } = [];
    public Guid AuthorId { get; set; }
    public User Author { get; set; }
    public Product Product { get; set; }
    public List<Comment> Comments { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public Review() { }
    public Review(ReviewDto reviewDto) 
    {
        ReviewId = reviewDto.ReviewId;
        UserRating = reviewDto.UserRating;
        CommentIds = reviewDto.CommentIds;
    }

}
