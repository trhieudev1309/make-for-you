using FUNews.BusinessLogic;
using FUNews.BusinessLogic.Entities;
using FUNews.DataAccess.DAO;
using FUNews.Repositories.Interfaces;

namespace FUNews.Repositories.Repository
{
    public class TagRepository : ITagRepository
    {
        private readonly TagDAO _dao;

        public TagRepository(ApplicationDbContext context)
        {
            _dao = new TagDAO(context);
        }

        public Tag? GetById(short id) => _dao.GetById(id);
        public IEnumerable<Tag> GetAll() => _dao.GetAll();

        public void Add(Tag tag) => _dao.Add(tag);
        public void Update(Tag tag) => _dao.Update(tag);
        public void Delete(short id) => _dao.Delete(id);
    }
}
