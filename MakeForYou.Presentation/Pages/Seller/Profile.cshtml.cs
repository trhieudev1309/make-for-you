using MakeForYou.BusinessLogic.DTOs;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Services.Implement;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MakeForYou.Presentation.Pages.Seller
{
    [Authorize(Roles = "Seller")]
    public class ProfileModel : PageModel
    {
        private readonly ISellerService _sellerService;
        private readonly PortfolioService _portfolioService;

        public ProfileModel(ISellerService sellerService, PortfolioService portfolioService)
        {
            _sellerService = sellerService;
            _portfolioService = portfolioService;
        }

        public MakeForYou.BusinessLogic.Entities.Seller? Seller { get; set; }

        private long GetSellerId() =>
            long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<IActionResult> OnGetAsync()
        {
            Seller = await _sellerService.GetProfileAsync(GetSellerId());
            if (Seller == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync(
            string? FullName, string? Email,
            string? SkillDescription, string? Bio,
            int? PriceRange, int? AvailabilityStatus)
        {
            await _sellerService.UpdateProfileAsync(GetSellerId(), new UpdateProfileRequest
            {
                FullName = FullName,
                Email = Email,
                SkillDescription = SkillDescription,
                Bio = Bio,
                PriceRange = PriceRange,
                AvailabilityStatus = AvailabilityStatus
            });

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddPortfolioAsync(
            string? Title, string? Description, string? ImageUrl)
        {
            if (string.IsNullOrWhiteSpace(Title) && string.IsNullOrWhiteSpace(ImageUrl))
            {
                TempData["Error"] = "Please provide at least a title or image URL.";
                return RedirectToPage();
            }

            await _portfolioService.AddItemAsync(GetSellerId(), new PortfolioItem
            {
                Title = Title,
                Description = Description,
                ImageUrl = ImageUrl,
                CreatedAt = DateTime.UtcNow
            });

            TempData["Success"] = "Portfolio piece added.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeletePortfolioAsync(long portfolioId)
        {
            await _portfolioService.DeleteItemAsync(GetSellerId(), portfolioId);
            TempData["Success"] = "Portfolio piece removed.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync(
            string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New passwords do not match.";
                return RedirectToPage();
            }

            var ok = await _sellerService.ChangePasswordAsync(GetSellerId(), CurrentPassword, NewPassword);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Password updated successfully."
                : "Current password is incorrect.";

            return RedirectToPage();
        }
    }
}