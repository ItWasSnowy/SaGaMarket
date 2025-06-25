using SaGaMarket.Core.Dtos;

namespace SaGaMarket.Core.Entities;

public class Review
{
    public Guid ReviewId { get; set; }
    public double UserRating { get; set; }
    public List<Guid> CommentIds { get; set; } = [];


    //Тут должны быть связи для EntityFramework


    public Review() { }
    public Review(ReviewDto reviewDto) 
    {
        ReviewId = reviewDto.ReviewId;
        UserRating = reviewDto.UserRating;
        CommentIds = reviewDto.CommentIds;
    }

}
