using FUNews.BusinessLogic.Entities;

namespace FUNews.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Category? GetById(short id);
        IEnumerable<Category> GetAll();

        void Add(Category category);
        void Update(Category category);
        void Delete(short id);

        bool IsUsedByArticles(short categoryId);
        bool HasSubCategories(short categoryId);

        IEnumerable<Category> Search(string keyword);
        Dictionary<short, int> GetArticleCountByCategory();

        void Activate(short categoryId);
        void Deactivate(short categoryId);
    }
}
