using MakeForYou.BusinessLogic; // Chứa ApplicationDbContext
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces; // Gọi interface từ BusinessLogic
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.Repositories.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;
        public CartRepository(ApplicationDbContext context) { _context = context; }

        public async Task<CartItem?> GetExistingItemAsync(long userId, long productId)
            => await _context.Set<CartItem>().FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

        public async Task AddItemAsync(CartItem item)
        {
            await _context.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(CartItem item)
        {
            _context.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCountByUserIdAsync(long userId)
            => await _context.Set<CartItem>()
                .Where(c => c.UserId == userId)
                .SumAsync(c => (int?)c.Quantity) ?? 0;

        public async Task<List<CartItem>> GetItemsByUserIdAsync(long userId)
        {
            return await _context.Set<CartItem>()
                .Include(c => c.Product) // JOIN với bảng Product
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task DeleteItemAsync(CartItem item)
        {
            _context.Set<CartItem>().Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(long userId)
        {
            var items = await _context.Set<CartItem>().Where(c => c.UserId == userId).ToListAsync();
            _context.Set<CartItem>().RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }


}