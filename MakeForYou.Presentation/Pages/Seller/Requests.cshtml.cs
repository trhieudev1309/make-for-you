using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MakeForYou.Presentation.Pages.Seller
{
    [Authorize(Roles = "Seller")]
    public class RequestsModel : PageModel
    {
        private readonly IOrderService _orderService;
        public RequestsModel(IOrderService orderService) => _orderService = orderService;

        public List<Order> Orders { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var sellerId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Orders = await _orderService.GetRequestsBySellerAsync(sellerId);
            return Page();
        }
    }
}