using FUNews.BusinessLogic.Entities;

namespace FUNews.Repositories.Interfaces
{
    public interface ITagRepository
    {
        Tag? GetById(short id);
        IEnumerable<Tag> GetAll();

        void Add(Tag tag);
        void Update(Tag tag);
        void Delete(short id);
    }
}
