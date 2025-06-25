using SaGaMarket.Core.Entities;

namespace SaGaMarket.Core.Dtos;

public class ReviewDto
{
    public Guid ReviewId { get; set; }
    public double UserRating { get; set; }
    public List<Guid> CommentIds { get; set; } = [];



    public ReviewDto() { }
    public ReviewDto(Review review)
    {
        ReviewId = review.ReviewId;
        UserRating = review.UserRating;
        CommentIds = review.CommentIds;
    }
}
