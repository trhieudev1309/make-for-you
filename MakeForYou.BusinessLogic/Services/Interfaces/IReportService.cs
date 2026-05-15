using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MakeForYou.BusinessLogic.ViewModels;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface IReportService
    {
        Task<DashboardViewModel> GetAdminDashboardStatsAsync();
    }
}
