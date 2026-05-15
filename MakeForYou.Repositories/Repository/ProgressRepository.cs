using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using Microsoft.EntityFrameworkCore;

public class ProgressRepository : IProgressRepository
{
    private readonly ApplicationDbContext _context;
    public ProgressRepository(ApplicationDbContext context) => _context = context;

    public async Task<OrderProgress> CreateAsync(OrderProgress progress)
    {
        _context.OrderProgresses.Add(progress);
        await _context.SaveChangesAsync();
        return progress;
    }

    public async Task<List<OrderProgress>> GetByOrderIdAsync(long orderId) =>
        await _context.OrderProgresses
                 .Where(p => p.OrderId == orderId)
                 .OrderByDescending(p => p.CreatedAt)
                 .ToListAsync();    
}