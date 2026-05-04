using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Checkout
{
    public class SuccessModel : PageModel
    {
        // Danh sách chứa các ID đơn hàng để hiển thị lên UI
        public List<string> OrderIdList { get; set; } = new();

        public void OnGet(string orderIds)
        {
            if (!string.IsNullOrEmpty(orderIds))
            {
                // Tách chuỗi "1,2,3" thành List {"1", "2", "3"}
                OrderIdList = orderIds.Split(',').ToList();
            }
        }
    }
}