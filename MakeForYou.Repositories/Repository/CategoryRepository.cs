using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;

namespace MakeForYou.Repositories.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext context) => _context = context;

        public async Task<List<Category>> GetAllAsync() => await _context.Categories.Include(c => c.Products).ToListAsync();

        public async Task<Category?> GetByIdAsync(long id) => await _context.Categories.FindAsync(id);

        public async Task<bool> AddAsync(Category category)
        {
            _context.Categories.Add(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var cat = await GetByIdAsync(id);
            if (cat == null) return false;
            _context.Categories.Remove(cat);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
