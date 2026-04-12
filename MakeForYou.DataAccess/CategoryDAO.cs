using FUNews.BusinessLogic;
using FUNews.BusinessLogic.Entities;
using Microsoft.EntityFrameworkCore;

namespace FUNews.DataAccess
{
    public class CategoryDAO
    {
        private readonly ApplicationDbContext _context;

        public CategoryDAO(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===== BASIC CRUD =====

        public Category? GetById(short categoryId)
        {
            return _context.Categories
                .Include(c => c.InverseParentCategory) // sub-categories
                .FirstOrDefault(c => c.CategoryId == categoryId);
        }

        public IEnumerable<Category> GetAll()
        {
            return _context.Categories
                .Include(c => c.InverseParentCategory)
                .ToList();
        }

        public void Add(Category category)
        {
            //var maxi = _context.Categories.Select(x => x.CategoryId).Max();
            //maxi += 1;
            //category.CategoryId = maxi;
            _context.Categories.Add(category);
            _context.SaveChanges();
        }

        public void Update(Category category)
        {
            _context.Categories.Update(category);
            _context.SaveChanges();
        }

        public void Delete(short categoryId)
        {
            var category = _context.Categories.Find(categoryId);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
        }

        // ===== BUSINESS CONSTRAINTS =====

        // Check if category is used by any news article
        public bool IsUsedByArticles(short categoryId)
        {
            return _context.NewsArticles
                .Any(n => n.CategoryId == categoryId);
        }

        // Check if category has sub-categories
        public bool HasSubCategories(short categoryId)
        {
            return _context.Categories
                .Any(c => c.ParentCategoryId == categoryId);
        }

        // ===== SEARCH =====

        public IEnumerable<Category> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return GetAll();

            return _context.Categories
                .Where(c =>
                    c.CategoryName.Contains(keyword) ||
                    c.CategoryDesciption.Contains(keyword))
                .ToList();
        }

        // ===== STATUS =====

        public void Activate(short categoryId)
        {
            var category = _context.Categories.Find(categoryId);
            if (category != null)
            {
                category.IsActive = true;
                _context.SaveChanges();
            }
        }

        public void Deactivate(short categoryId)
        {
            var category = _context.Categories.Find(categoryId);
            if (category != null)
            {
                category.IsActive = false;
                _context.SaveChanges();
            }
        }
    }
}
