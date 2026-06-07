using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.Enums;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class AdminOrdersModel : PageModel
{
    private readonly IOrderService _orderService;
    private readonly IPayoutService _payoutService;

    public AdminOrdersModel(IOrderService orderService, IPayoutService payoutService)
    {
        _orderService = orderService;
        _payoutService = payoutService;
    }

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
        if (status == (int)OrderStatus.Done)
            await _payoutService.PaySellerAsync(orderId);
        return RedirectToPage();
    }

    public static string StatusLabel(int status) => status switch
    {
        0  => "Chờ xác nhận",
        1  => "Đã xác nhận",
        2  => "Đã báo giá",
        3  => "Đang thực hiện",
        4  => "Hoàn thành",
        5  => "Đang giao hàng",
        6  => "Đã giao hàng",
        7  => "Đã xong",
        8  => "Đã hủy",
        9  => "Chờ nghệ nhân báo giá",
        10 => "Chờ chấp nhận báo giá",
        11 => "Chờ thanh toán báo giá",
        _  => "Không xác định"
    };
}