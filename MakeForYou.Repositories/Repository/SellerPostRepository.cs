using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.Repositories.Repository
{
    public class SellerPostRepository : ISellerPostRepository
    {
        private readonly ApplicationDbContext _context;

        public SellerPostRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SellerPost>> GetBySellerIdAsync(long sellerId) =>
            await _context.SellerPosts
                .Where(p => p.SellerId == sellerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

        public async Task<SellerPost?> GetByIdAsync(long postId) =>
            await _context.SellerPosts
                .Include(p => p.Seller).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.PostId == postId);

        public async Task<SellerPost> AddAsync(SellerPost post)
        {
            _context.SellerPosts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task UpdateAsync(SellerPost post)
        {
            _context.SellerPosts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(long postId, long sellerId)
        {
            var post = await _context.SellerPosts
                .FirstOrDefaultAsync(p => p.PostId == postId && p.SellerId == sellerId);

            if (post == null)
                return false;

            _context.SellerPosts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
