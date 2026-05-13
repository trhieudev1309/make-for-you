using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class AdminOrdersModel : PageModel
{
    private readonly IOrderService _orderService;
    public AdminOrdersModel(IOrderService orderService) => _orderService = orderService;

    public List<Order> OrderList { get; set; } = new();

    [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
    [BindProperty(SupportsGet = true)] public int? StatusFilter { get; set; }
    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }

    public async Task OnGetAsync()
    {
        var (orders, total) = await _orderService.GetAllOrdersForAdminAsync(
            SearchTerm, StatusFilter, CurrentPage, 10);

        OrderList = orders;
        TotalPages = (int)Math.Ceiling(total / 10.0);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(long orderId, int status)
    {
        await _orderService.UpdateStatusAsync(orderId, status);
        return RedirectToPage();
    }
}