using FUNews.BusinessLogic.Entities;
using FUNews.DataAccess;
using FUNews.Repositories.Interfaces;

namespace FUNews.Repositories.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CategoryDAO _dao;

        public CategoryRepository(CategoryDAO dao)
        {
            _dao = dao;
        }

        public Category? GetById(short id)
            => _dao.GetById(id);

        public IEnumerable<Category> GetAll()
            => _dao.GetAll();

        public void Add(Category category)
            => _dao.Add(category);

        public void Update(Category category)
            => _dao.Update(category);

        public void Delete(short id)
            => _dao.Delete(id);

        public bool IsUsedByArticles(short categoryId)
            => _dao.IsUsedByArticles(categoryId);

        public bool HasSubCategories(short categoryId)
            => _dao.HasSubCategories(categoryId);

        public IEnumerable<Category> Search(string keyword)
            => _dao.Search(keyword);

        public void Activate(short categoryId)
            => _dao.Activate(categoryId);

        public void Deactivate(short categoryId)
            => _dao.Deactivate(categoryId);

        public Dictionary<short, int> GetArticleCountByCategory()
        {
            var allArticles = _dao.GetAll();
            return allArticles
                .GroupBy(n => n.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Count = g.Count()
                })
                .ToDictionary(x => x.CategoryId, x => x.Count);
        }

    }
}
