using SaGaMarket.Core.Entities;

namespace TourGuide.Core.Storage.Repositories
{
    public interface ICommentRepository
    {
        public Task<Guid> Create(Comment comment);
        public Task<bool> Update(Comment comment); 

        //public Task<Guid> Create(Comment comment);
        //public Task<bool> Update(Comment comment);
        public Task Delete(Guid commentId);

        public Task<Comment?> Get(Guid commentId);
        public Task<List<Comment>?> GetByAuthor(Guid AuthorId);
        public Task<List<Comment>?> GetByTourRoute(Guid tourRouteId);
    }
}
