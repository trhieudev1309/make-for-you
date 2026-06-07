using System.Security.Claims;
using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Entities.Enums;
using MakeForYou.BusinessLogic.Services.Interfaces;
using MakeForYou.BusinessLogic.Services;
using MakeForYou.BusinessLogic.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MakeForYou.Presentation.Pages.Orders
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _db;
        private readonly IQuotationService _quotationService;
        private readonly IPaymentService _paymentService;
        private readonly IPayoutService _payoutService;
        private readonly IGhnService _ghnService;

        public DetailModel(
            IOrderService orderService, 
            ApplicationDbContext db, 
            IQuotationService quotationService, 
            IPaymentService paymentService, 
            IPayoutService payoutService,
            IGhnService ghnService)
        {
            _orderService = orderService;
            _db = db;
            _quotationService = quotationService;
            _paymentService = paymentService;
            _payoutService = payoutService;
            _ghnService = ghnService;
        }

        public Order? Order { get; set; }
        public List<GhnLogDto> TrackingLogs { get; set; } = new();

        // Bindings for feedback form
        [BindProperty]
        public int FeedbackRating { get; set; }

        [BindProperty]
        public string? FeedbackComment { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            var buyerId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            Order = await _orderService.GetOrderDetailAsync(id, buyerId);

            if (Order == null) return NotFound();

            // 2. XỬ LÝ ĐOẠN HOÃN (MOCK DATA CHO GIAI ĐOẠN TEST)
            if (!string.IsNullOrEmpty(Order.GhnShipmentCode))
            {
                TrackingLogs = await _ghnService.GetOrderTrackingLogsAsync(Order.GhnShipmentCode);
            }
            else
            {
                // Chỉ hiển thị lịch trình giả lập để demo ở môi trường Local / Development
                var isDevelopment = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);
                if (isDevelopment)
                {
                    // Giả lập lịch trình động theo trạng thái thực tế của đơn hàng trong database
                    TrackingLogs = new List<GhnLogDto>();

                    var status = (OrderStatus)Order.Status;

                    if (status == OrderStatus.Cancelled)
                    {
                        TrackingLogs.Add(new GhnLogDto { Status = "cancel", UpdatedDate = Order.CreatedAt.AddMinutes(30) });
                    }
                    else
                    {
                        // Bước 1: Đang chờ lấy hàng (Trạng thái Pending hoặc lớn hơn)
                        TrackingLogs.Add(new GhnLogDto { Status = "ready_to_pick", UpdatedDate = Order.CreatedAt.AddMinutes(15) });

                        if (status == OrderStatus.Confirmed || status == OrderStatus.InProgress || status == OrderStatus.Delivering || status == OrderStatus.Delivered || status == OrderStatus.Completed || status == OrderStatus.Done)
                        {
                            // Bước 2: Nhân viên đang đến lấy hàng
                            TrackingLogs.Add(new GhnLogDto { Status = "picking", UpdatedDate = Order.CreatedAt.AddHours(2) });
                        }

                        if (status == OrderStatus.InProgress || status == OrderStatus.Delivering || status == OrderStatus.Delivered || status == OrderStatus.Completed || status == OrderStatus.Done)
                        {
                            // Bước 3: Đã lấy hàng thành công
                            TrackingLogs.Add(new GhnLogDto { Status = "picked", UpdatedDate = Order.CreatedAt.AddHours(4) });
                        }

                        if (status == OrderStatus.Delivering || status == OrderStatus.Delivered || status == OrderStatus.Completed || status == OrderStatus.Done)
                        {
                            // Bước 4: Đang giao hàng
                            TrackingLogs.Add(new GhnLogDto { Status = "delivering", UpdatedDate = Order.CreatedAt.AddHours(6) });
                        }

                        if (status == OrderStatus.Delivered || status == OrderStatus.Completed || status == OrderStatus.Done)
                        {
                            // Bước 5: Giao hàng thành công
                            TrackingLogs.Add(new GhnLogDto { Status = "delivered", UpdatedDate = Order.CreatedAt.AddHours(24) });
                        }
                    }
                }
            }

            return Page();
        }

        // Handler to receive feedback submissions
        public async Task<IActionResult> OnPostFeedbackAsync(long id)
        {
            var buyerId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Re-load order and enforce ownership & status
            var order = await _orderService.GetOrderDetailAsync(id, buyerId);
            if (order == null) return NotFound();

            if ((OrderStatus)order.Status != OrderStatus.Completed)
            {
                ModelState.AddModelError(string.Empty, "You can only leave feedback for completed orders.");
                Order = order;
                return Page();
            }

            // Validate rating
            if (FeedbackRating < 0 || FeedbackRating > 5)
            {
                ModelState.AddModelError(nameof(FeedbackRating), "Rating must be between 0 and 5.");
                Order = order;
                return Page();
            }

            // Prevent duplicate feedback from same buyer for same order
            if (order.Reviews != null && order.Reviews.Any(r => r.BuyerId == buyerId))
            {
                ModelState.AddModelError(string.Empty, "You have already left feedback for this order.");
                Order = order;
                return Page();
            }

            // Create and persist review
            var review = new Review
            {
                BuyerId = buyerId,
                SellerId = order.SellerId,
                OrderId = order.OrderId,
                Rating = FeedbackRating,
                Comment = FeedbackComment ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _db.Reviews.Add(review);

            // Update seller aggregates (TotalReviews, AverageRating) safely
            var seller = await _db.Sellers.FindAsync(order.SellerId);
            if (seller != null)
            {
                var prevCount = seller.TotalReviews ?? 0;
                var prevAvg = seller.AverageRating ?? 0;
                var newCount = prevCount + 1;
                var newAvg = (int)Math.Round(((prevAvg * prevCount) + FeedbackRating) / (double)newCount, MidpointRounding.AwayFromZero);

                seller.TotalReviews = newCount;
                seller.AverageRating = newAvg;
            }

            await _db.SaveChangesAsync();

            // Redirect to GET to refresh view and avoid reposts
            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostCancelAsync(long id)
        {
            var buyerId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await _orderService.GetOrderDetailAsync(id, buyerId);
            if (order == null) return NotFound();

            if (order.Status == (int)OrderStatus.Pending || order.Status == (int)OrderStatus.Quoted)
            {
                await _orderService.UpdateStatusAsync(id, (int)OrderStatus.Cancelled);
                TempData["SuccessMessage"] = "Hủy đơn hàng thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng ở trạng thái hiện tại.";
            }

            return RedirectToPage(new { id = id });
        }

        // Matches the custom CSS badge classes in Detail.cshtml
        public static string BadgeClass(int status) => (OrderStatus)status switch
        {
            OrderStatus.Pending                => "badge-secondary",
            OrderStatus.Quoted                 => "badge-info",
            OrderStatus.Confirmed              => "badge-primary",
            OrderStatus.InProgress             => "badge-warning",
            OrderStatus.Completed              => "badge-success",
            OrderStatus.Delivering             => "badge-info",
            OrderStatus.Delivered              => "badge-primary",
            OrderStatus.Done                   => "badge-success",
            OrderStatus.Cancelled              => "badge-danger",
            OrderStatus.PendingQuotationSubmit  => "badge-warning",
            OrderStatus.PendingQuotationAccept  => "badge-info",
            OrderStatus.PendingQuotationPayment => "badge-info",
            _                                  => "badge-secondary"
        };

        public static string StatusIcon(int status) => (OrderStatus)status switch
        {
            OrderStatus.Pending                => "bi-hourglass",
            OrderStatus.Quoted                 => "bi-tag",
            OrderStatus.Confirmed              => "bi-check-circle",
            OrderStatus.InProgress             => "bi-tools",
            OrderStatus.Completed              => "bi-bag-check",
            OrderStatus.Delivering             => "bi-truck",
            OrderStatus.Delivered              => "bi-house-check",
            OrderStatus.Done                   => "bi-star-fill",
            OrderStatus.Cancelled              => "bi-x-circle",
            OrderStatus.PendingQuotationSubmit  => "bi-pencil-square",
            OrderStatus.PendingQuotationAccept  => "bi-tag",
            OrderStatus.PendingQuotationPayment => "bi-credit-card",
            _                                  => "bi-circle"
        };

        public static string StatusLabel(int status) => (OrderStatus)status switch
        {
            OrderStatus.Pending                => "Chờ xác nhận",
            OrderStatus.Confirmed              => "Đã xác nhận",
            OrderStatus.Quoted                 => "Đã báo giá",
            OrderStatus.InProgress             => "Đang thực hiện",
            OrderStatus.Completed              => "Hoàn thành",
            OrderStatus.Delivering             => "Đang giao hàng",
            OrderStatus.Delivered              => "Đã giao hàng",
            OrderStatus.Done                   => "Đã xong",
            OrderStatus.Cancelled              => "Đã hủy",
            OrderStatus.PendingQuotationSubmit  => "Chờ nghệ nhân báo giá",
            OrderStatus.PendingQuotationAccept  => "Chờ chấp nhận báo giá",
            OrderStatus.PendingQuotationPayment => "Chờ thanh toán báo giá",
            _                                  => "Không xác định"
        };
    }
}