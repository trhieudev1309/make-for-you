using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MakeForYou.BusinessLogic.Services.Interfaces;
using MakeForYou.BusinessLogic.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        public ReportService(ApplicationDbContext context) => _context = context;

        public async Task<DashboardViewModel> GetAdminDashboardStatsAsync()
        {
            var stats = new DashboardViewModel();

            // 1. Thống kê tổng quan từ các bảng có sẵn
            stats.TotalUsers = await _context.Users.CountAsync();
            stats.TotalOrders = await _context.Orders.CountAsync();
            stats.TotalProducts = await _context.Products.CountAsync();

            // Chỉ tính tiền từ các đơn hàng đã hoàn thành (Status = 4)
            stats.TotalRevenue = await _context.Orders
                .Where(o => o.Status == 4)
                .SumAsync(o => (long)(o.AgreedPrice ?? 0));

            // 2. Lấy dữ liệu 7 ngày gần nhất cho biểu đồ
            var startDate = DateTime.UtcNow.Date.AddDays(-6);
            var dailyStats = await _context.Orders
                .Where(o => o.Status == 4 && o.CompletedAt >= startDate)
                .GroupBy(o => o.CompletedAt!.Value.Date)
                .Select(g => new { Date = g.Key, DailySum = g.Sum(o => (long)(o.AgreedPrice ?? 0)) })
                .ToListAsync();

            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i);
                stats.Labels.Add(date.ToString("dd/MM"));
                var sum = dailyStats.FirstOrDefault(d => d.Date == date)?.DailySum ?? 0;
                stats.RevenueData.Add(sum);
            }

            return stats;
        }
    }
}
