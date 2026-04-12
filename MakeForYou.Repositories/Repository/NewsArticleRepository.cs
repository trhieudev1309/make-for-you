using FUNews.BusinessLogic.Entities;
using FUNews.DataAccess;
using FUNews.Repositories.Interfaces;

namespace FUNews.Repositories.Repository
{
    public class NewsArticleRepository : INewsArticleRepository
    {
        private readonly NewsArticleDAO _dao;

        public NewsArticleRepository(NewsArticleDAO dao)
        {
            _dao = dao;
        }

        // ===== BASIC CRUD =====

        public NewsArticle? GetById(string id)
            => _dao.GetById(id);

        public IEnumerable<NewsArticle> GetAll()
            => _dao.GetAll();

        public void Add(NewsArticle article)
            => _dao.Add(article);

        public void Update(NewsArticle article)
            => _dao.Update(article);

        public void Delete(string id)
            => _dao.Delete(id);

        // ===== SEARCH & FILTER =====

        public IEnumerable<NewsArticle> Search(string keyword)
            => _dao.Search(keyword);

        public IEnumerable<NewsArticle> GetByCategory(short categoryId)
            => _dao.GetByCategory(categoryId);

        public IEnumerable<NewsArticle> GetByCreator(short creatorId)
            => _dao.GetByCreator(creatorId);

        public IEnumerable<NewsArticle> GetByStatus(bool status)
            => _dao.GetByStatus(status);

        public IEnumerable<NewsArticle> GetByCreatedDateRange(
            DateTime start,
            DateTime end)
            => _dao.GetByCreatedDateRange(start, end);

        // ===== TAG =====

        public void AddTag(string newsArticleId, int tagId)
            => _dao.AddTag(newsArticleId, tagId);

        public void RemoveTag(string newsArticleId, int tagId)
            => _dao.RemoveTag(newsArticleId, tagId);

        public IEnumerable<NewsArticle> GetRelatedArticles(
            string newsArticleId,
            int limit = 3)
            => _dao.GetRelatedArticles(newsArticleId, limit);

        // ===== REPORTING & STATISTICS (REPO ONLY) =====

        public int CountByStatus(
            bool status,
            DateTime startDate,
            DateTime endDate)
        {
            return _dao.GetByStatus(status)
                .Count(n =>
                    n.CreatedDate.HasValue &&
                    n.CreatedDate.Value >= startDate &&
                    n.CreatedDate.Value <= endDate);
        }

        public IDictionary<string, int> CountByCategory(
            DateTime startDate,
            DateTime endDate)
        {
            return _dao.GetByCreatedDateRange(startDate, endDate)
                .Where(n => n.Category != null)
                .GroupBy(n => n.Category!.CategoryName)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public IDictionary<string, int> CountByAuthor(
            DateTime startDate,
            DateTime endDate)
        {
            return _dao.GetByCreatedDateRange(startDate, endDate)
                .Where(n => n.CreatedBy != null)
                .GroupBy(n => n.CreatedBy.AccountName)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
