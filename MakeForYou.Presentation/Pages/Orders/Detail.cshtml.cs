using System.Security.Claims;
using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Enums;
using MakeForYou.BusinessLogic.Services.Interfaces;
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

        public DetailModel(IOrderService orderService, ApplicationDbContext db)
        {
            _orderService = orderService;
            _db = db;
        }

        public Order? Order { get; set; }

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

        public static string BadgeClass(int status) => (OrderStatus)status switch
        {
            OrderStatus.Pending => "bg-secondary",
            OrderStatus.Quoted => "bg-info text-dark",
            OrderStatus.Confirmed => "bg-primary",
            OrderStatus.InProgress => "bg-warning text-dark",
            OrderStatus.Completed => "bg-success",
            OrderStatus.Cancelled => "bg-danger",
            _ => "bg-secondary"
        };

        public static string StatusIcon(int status) => (OrderStatus)status switch
        {
            OrderStatus.Pending => "bi-hourglass",
            OrderStatus.Quoted => "bi-tag",
            OrderStatus.Confirmed => "bi-check-circle",
            OrderStatus.InProgress => "bi-tools",
            OrderStatus.Completed => "bi-bag-check",
            OrderStatus.Cancelled => "bi-x-circle",
            _ => "bi-circle"
        };
    }
}