using FUNews.BusinessLogic.Entities;

namespace FUNews.Repositories.Interfaces
{
    public interface INewsArticleRepository
    {
        NewsArticle? GetById(string id);
        IEnumerable<NewsArticle> GetAll();

        void Add(NewsArticle article);
        void Update(NewsArticle article);
        void Delete(string id);

        IEnumerable<NewsArticle> Search(string keyword);
        IEnumerable<NewsArticle> GetByCategory(short categoryId);
        IEnumerable<NewsArticle> GetByCreator(short creatorId);
        IEnumerable<NewsArticle> GetByStatus(bool status);
        IEnumerable<NewsArticle> GetByCreatedDateRange(DateTime start, DateTime end);

        void AddTag(string newsArticleId, int tagId);
        void RemoveTag(string newsArticleId, int tagId);

        IEnumerable<NewsArticle> GetRelatedArticles(string newsArticleId, int limit = 3);

        int CountByStatus(
            bool status,
            DateTime start,
            DateTime end
        );

        IDictionary<string, int>
            CountByCategory(DateTime start, DateTime end);

        IDictionary<string, int>
            CountByAuthor(DateTime start, DateTime end);

    }
}
