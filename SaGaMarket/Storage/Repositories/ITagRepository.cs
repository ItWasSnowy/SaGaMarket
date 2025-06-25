using SaGaMarket.Core.Entities;

namespace TourGuide.Core.Storage.Repositories
{
    public interface ITagRepository
    {
        Task<string> Create(Tag tag);
        Task<bool> Update(Tag tag);
        Task Delete(string tagId);
        Task<Tag?> Get(string tagId);
        
    }

}
