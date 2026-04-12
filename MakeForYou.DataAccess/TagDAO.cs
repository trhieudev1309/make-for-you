using FUNews.BusinessLogic;
using FUNews.BusinessLogic.Entities;
using Microsoft.EntityFrameworkCore;

namespace FUNews.DataAccess.DAO
{
    public class TagDAO
    {
        private readonly ApplicationDbContext _context;

        public TagDAO(ApplicationDbContext context)
        {
            _context = context;
        }

        public Tag? GetById(int tagId)
            => _context.Tags.Find(tagId);

        public IEnumerable<Tag> GetAll()
            => _context.Tags.ToList();

        public void Add(Tag tag)
        {
            var maxi = _context.Tags.Select(x => x.TagId).Max();
            maxi += 1;
            tag.TagId = maxi;
            _context.Tags.Add(tag);
            _context.SaveChanges();
        }

        public void Update(Tag tag)
        {
            _context.Tags.Update(tag);
            _context.SaveChanges();
        }

        public void Delete(int tagId)
        {
            var tag = _context.Tags
                .Include(t => t.NewsArticles)
                .FirstOrDefault(t => t.TagId == tagId);

            if (tag != null && !tag.NewsArticles.Any())
            {
                _context.Tags.Remove(tag);
                _context.SaveChanges();
            }
        }

        // ===== VALIDATION =====

        public bool IsTagNameExists(string tagName, int? excludeTagId = null)
        {
            return _context.Tags.Any(t =>
                t.TagName == tagName &&
                (!excludeTagId.HasValue || t.TagId != excludeTagId));
        }

        public IEnumerable<Tag> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAll();

            return _context.Tags
                .Where(t => t.TagName.Contains(keyword))
                .ToList();
        }
    }
}
