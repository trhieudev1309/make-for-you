using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.ViewModels;

namespace MakeForYou.Presentation.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public DashboardViewModel Stats { get; set; } = new();

        [BindProperty(SupportsGet = true)] public string DateRange { get; set; } = "7days";
        [BindProperty(SupportsGet = true)] public DateTime? StartDate { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? EndDate { get; set; }

        public async Task OnGetAsync()
        {
            // 1. Xác định mốc thời gian kỳ này và kỳ trước
            var (cS, cE, pS, pE) = GetDateBounds(DateRange, StartDate, EndDate);

            // 2. Lấy dữ liệu đơn hàng
            var currentOrders = await _context.Orders
                .Where(o => o.Status == 4 && o.CompletedAt >= cS && o.CompletedAt <= cE)
                .Select(o => new { o.AgreedPrice, o.CompletedAt, SellerName = o.Seller.User.FullName })
                .ToListAsync();

            var prevOrders = await _context.Orders
                .Where(o => o.Status == 4 && o.CompletedAt >= pS && o.CompletedAt <= pE)
                .Select(o => new { o.AgreedPrice })
                .ToListAsync();

            // 3. Stats tổng quan & Tăng trưởng
            Stats.TotalOrders = currentOrders.Count;
            Stats.TotalRevenue = currentOrders.Sum(o => (long)(o.AgreedPrice ?? 0));
            Stats.TotalUsers = await _context.Users.CountAsync();
            Stats.TotalProducts = await _context.Products.CountAsync();

            Stats.RevenueGrowth = CalculateGrowth(Stats.TotalRevenue, prevOrders.Sum(o => (long)(o.AgreedPrice ?? 0)));
            Stats.OrderGrowth = CalculateGrowth(Stats.TotalOrders, prevOrders.Count);

            // 4. Top 5 Nghệ nhân
            Stats.TopSellers = currentOrders
                .GroupBy(o => o.SellerName)
                .Select(g => new TopSellerDto
                {
                    Name = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => (long)(o.AgreedPrice ?? 0))
                })
                .OrderByDescending(x => x.Revenue).Take(5).ToList();

            // 5. THÊM MỚI: Thống kê trạng thái đơn hàng (Toàn thời gian để quản trị pipeline)
            Stats.OrderStatusCounts = new List<int>();
            for (int i = 0; i <= 5; i++)
            {
                var count = await _context.Orders.CountAsync(o => o.Status == i);
                Stats.OrderStatusCounts.Add(count);
            }

            // 6. Logic biểu đồ Doanh thu (Sửa lỗi hiển thị năm)
            if (DateRange == "year")
            {
                for (int m = 1; m <= 12; m++)
                {
                    Stats.Labels.Add($"Th {m}");
                    var totalInMonth = currentOrders
                        .Where(o => o.CompletedAt!.Value.Month == m && o.CompletedAt.Value.Year == cS.Year)
                        .Sum(o => (long)(o.AgreedPrice ?? 0));
                    Stats.RevenueData.Add(totalInMonth);
                }
            }
            else
            {
                for (DateTime date = cS.Date; date <= cE.Date; date = date.AddDays(1))
                {
                    Stats.Labels.Add(date.ToString("dd/MM"));
                    var totalInDay = currentOrders
                        .Where(o => o.CompletedAt!.Value.Date == date.Date)
                        .Sum(o => (long)(o.AgreedPrice ?? 0));
                    Stats.RevenueData.Add(totalInDay);
                }
            }
        }

        private (DateTime cS, DateTime cE, DateTime pS, DateTime pE) GetDateBounds(string range, DateTime? start, DateTime? end)
        {
            DateTime now = DateTime.UtcNow;
            DateTime cS, cE = end ?? now, pS, pE;

            if (range == "custom" && start.HasValue)
            {
                cS = start.Value;
                TimeSpan diff = cE - cS;
                pE = cS.AddDays(-1);
                pS = pE - diff;
            }
            else
            {
                switch (range)
                {
                    case "today": cS = now.Date; break;
                    case "month": cS = new DateTime(now.Year, now.Month, 1); break;
                    case "year": cS = new DateTime(now.Year, 1, 1); break;
                    default: cS = now.AddDays(-7); break;
                }
                pE = cS.AddDays(-1);
                pS = pE - (cE.Date - cS.Date);
            }
            return (cS, cE, pS, pE);
        }

        private double CalculateGrowth(long c, long p) => p == 0 ? (c > 0 ? 100 : 0) : Math.Round(((double)(c - p) / p) * 100, 1);
    }
}