using FUNews.BusinessLogic;
using FUNews.BusinessLogic.Entities;
using Microsoft.EntityFrameworkCore;

namespace FUNews.DataAccess
{
    public class NewsArticleDAO
    {
        private readonly ApplicationDbContext _context;

        public NewsArticleDAO(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===== BASIC CRUD =====

        public NewsArticle? GetById(string newsArticleId)
        {
            return _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .FirstOrDefault(n => n.NewsArticleId == newsArticleId);
        }

        public IEnumerable<NewsArticle> GetAll()
        {
            return _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .ToList();
        }

        public void Add(NewsArticle article)
        {
            var maxi = _context.NewsArticles.Select(x => x.NewsArticleId).ToList();
            var maxInt = maxi.Select(x => Int32.Parse(x)).Max();
            maxInt += 1;
            article.NewsArticleId = maxInt.ToString();
            _context.NewsArticles.Add(article);
            _context.SaveChanges();
        }

        public void Update(NewsArticle article)
        {
            _context.NewsArticles.Update(article);
            _context.SaveChanges();
        }

        public void Delete(string newsArticleId)
        {
            var article = _context.NewsArticles
                .Include(n => n.Tags)
                .FirstOrDefault(n => n.NewsArticleId == newsArticleId);

            if (article != null)
            {
                // Remove links in implicit join table
                article.Tags.Clear();

                _context.NewsArticles.Remove(article);
                _context.SaveChanges();
            }
        }

        // ===== SEARCH & FILTER =====

        public IEnumerable<NewsArticle> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAll();

            return _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Where(n =>
                    (n.NewsTitle != null && n.NewsTitle.Contains(keyword)) ||
                    n.Headline.Contains(keyword) ||
                    (n.Category != null && n.Category.CategoryName.Contains(keyword)) ||
                    (n.CreatedBy != null && n.CreatedBy.AccountName.Contains(keyword)))
                .ToList();
        }

        public IEnumerable<NewsArticle> GetByCreator(short createdById)
        {
            return _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Where(n => n.CreatedById == createdById)
                .ToList();
        }

        public IEnumerable<NewsArticle> GetByCategory(short categoryId)
        {
            return _context.NewsArticles
                .Include(n => n.Category)
                .Where(n => n.CategoryId == categoryId)
                .ToList();
        }

        public IEnumerable<NewsArticle> GetByStatus(bool status)
        {
            return _context.NewsArticles
                .Where(n => n.NewsStatus == status)
                .ToList();
        }

        // ===== DATE FILTERING =====

        public IEnumerable<NewsArticle> GetByCreatedDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.NewsArticles
                .Include(x => x.Category)
                .Include(x => x.CreatedBy)
                .Where(n =>
                    n.CreatedDate.HasValue &&
                    n.CreatedDate.Value >= startDate &&
                    n.CreatedDate.Value <= endDate)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();
        }

        // ===== TAG MANAGEMENT =====

        public void AddTag(string newsArticleId, int tagId)
        {
            var article = _context.NewsArticles
                .Include(n => n.Tags)
                .FirstOrDefault(n => n.NewsArticleId == newsArticleId);

            if (article == null)
                return;

            var tag = _context.Tags.FirstOrDefault(x => x.TagId == tagId);
            if (tag == null)
                return;

            if (!article.Tags.Any(t => t.TagId == tagId))
            {
                article.Tags.Add(tag);
                _context.SaveChanges();
            }
        }

        public void RemoveTag(string newsArticleId, int tagId)
        {
            var article = _context.NewsArticles
                .Include(n => n.Tags)
                .FirstOrDefault(n => n.NewsArticleId == newsArticleId);

            if (article != null)
            {
                var tag = article.Tags.FirstOrDefault(t => t.TagId == tagId);
                if (tag != null)
                {
                    article.Tags.Remove(tag);
                    _context.SaveChanges();
                }
            }
        }

        // ===== RELATED ARTICLES =====

        public IEnumerable<NewsArticle> GetRelatedArticles(string newsArticleId, int limit = 3)
        {
            var article = _context.NewsArticles
                .Include(n => n.Tags)
                .FirstOrDefault(n => n.NewsArticleId == newsArticleId);

            if (article == null)
                return Enumerable.Empty<NewsArticle>();

            var tagIds = article.Tags.Select(t => t.TagId).ToList();

            return _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.Tags)
                .Where(n =>
                    n.NewsArticleId != newsArticleId &&
                    n.NewsStatus == true &&
                    (
                        (article.CategoryId.HasValue && n.CategoryId == article.CategoryId) ||
                        n.Tags.Any(t => tagIds.Contains(t.TagId))
                    ))
                .Take(limit)
                .ToList();
        }
    }
}
