using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public long TotalRevenue { get; set; }
        public int TotalProducts { get; set; }

        // Tỉ lệ tăng trưởng (%)
        public double RevenueGrowth { get; set; }
        public double OrderGrowth { get; set; }
        public double UserGrowth { get; set; }

        // Dữ liệu biểu đồ
        public List<string> Labels { get; set; } = new();
        public List<long> RevenueData { get; set; } = new();
        public List<int> OrderStatusCounts { get; set; } = new();

        // Top Nghệ nhân
        public List<TopSellerDto> TopSellers { get; set; } = new();
    }

    public class TopSellerDto
    {
        public string Name { get; set; }
        public int OrderCount { get; set; }
        public long Revenue { get; set; }
    }
}
