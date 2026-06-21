using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.DTOs;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Services.Implement;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MakeForYou.Presentation.Pages.Seller
{
    [Authorize(Roles = "Seller")]
    public class ProfileModel : PageModel
    {
        private readonly ISellerService _sellerService;
        private readonly PortfolioService _portfolioService;
        private readonly ISellerPostService _postService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;

        public ProfileModel(
            ISellerService sellerService,
            PortfolioService portfolioService,
            ISellerPostService postService,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext context)
        {
            _sellerService = sellerService;
            _portfolioService = portfolioService;
            _postService = postService;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        public MakeForYou.BusinessLogic.Entities.Seller? Seller { get; set; }
        public List<SellerPost> Posts { get; set; } = new();
        public List<Product> Products { get; set; } = new();
        /// <summary>Which tab to activate on page load.</summary>
        public string ActiveTab { get; set; } = "profile";

        private long GetSellerId() =>
            long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<IActionResult> OnGetAsync()
        {
            var sellerId = GetSellerId();
            Seller = await _sellerService.GetProfileAsync(sellerId);
            if (Seller == null) return NotFound();
            Posts = await _postService.GetPostsBySellerAsync(sellerId);
            Products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.SellerId == sellerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            ActiveTab = TempData["ActiveTab"]?.ToString() ?? "profile";
            return Page();
        }

        // ── Profile ────────────────────────────────────────────────────────────
        public async Task<IActionResult> OnPostUpdateProfileAsync(
            string? FullName, string? Email,
            string? SkillDescription, string? Bio,
            int? PriceRange, int? AvailabilityStatus,
            IFormFile? Avatar, IFormFile? Cover)
        {
            await _sellerService.UpdateProfileAsync(GetSellerId(), new UpdateProfileRequest
            {
                FullName = FullName,
                Email = Email,
                SkillDescription = SkillDescription,
                Bio = Bio,
                PriceRange = PriceRange,
                AvailabilityStatus = AvailabilityStatus,
                Avatar = Avatar,
                Cover = Cover
            });
            TempData["Success"] = "Profile updated successfully.";
            TempData["ActiveTab"] = "profile";
            return RedirectToPage();
        }

        // ── Portfolio ───────────────────────────────────────────────────────────
        public async Task<IActionResult> OnPostAddPortfolioAsync(
            string? Title, string? Description, IFormFile? ImageFile)
        {
            if (string.IsNullOrWhiteSpace(Title) && ImageFile == null)
            {
                TempData["Error"] = "Vui lòng nhập tiêu đề hoặc tải lên hình ảnh.";
                TempData["ActiveTab"] = "portfolio";
                return RedirectToPage();
            }

            string? imageUrl = null;
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string folder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "portfolio");
                Directory.CreateDirectory(folder);
                string fileName = Guid.NewGuid() + "_" + Path.GetFileName(ImageFile.FileName);
                using (var fs = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
                    await ImageFile.CopyToAsync(fs);
                imageUrl = "/uploads/portfolio/" + fileName;
            }

            await _portfolioService.AddItemAsync(GetSellerId(), new PortfolioItem
            {
                Title = Title,
                Description = Description,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            });
            TempData["Success"] = "Đã thêm tác phẩm vào danh mục.";
            TempData["ActiveTab"] = "portfolio";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeletePortfolioAsync(long portfolioId)
        {
            await _portfolioService.DeleteItemAsync(GetSellerId(), portfolioId);
            TempData["Success"] = "Portfolio piece removed.";
            TempData["ActiveTab"] = "portfolio";
            return RedirectToPage();
        }

        // ── Account ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> OnPostChangePasswordAsync(
            string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New passwords do not match.";
                TempData["ActiveTab"] = "account";
                return RedirectToPage();
            }
            var ok = await _sellerService.ChangePasswordAsync(GetSellerId(), CurrentPassword, NewPassword);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Password updated successfully."
                : "Current password is incorrect.";
            TempData["ActiveTab"] = "account";
            return RedirectToPage();
        }

        // ── Posts ────────────────────────────────────────────────────────────────
        public async Task<IActionResult> OnPostCreatePostAsync(
            string Title, string Content, IFormFile? Image)
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Content))
            {
                TempData["Error"] = "Title and content are required.";
                TempData["ActiveTab"] = "posts";
                return RedirectToPage();
            }
            var result = await _postService.CreatePostAsync(GetSellerId(), new CreateSellerPostRequest
            {
                Title = Title.Trim(),
                Content = Content.Trim(),
                Image = Image
            });
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            TempData["ActiveTab"] = "posts";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdatePostAsync(
            long PostId, string Title, string Content, IFormFile? Image)
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Content))
            {
                TempData["Error"] = "Title and content are required.";
                TempData["ActiveTab"] = "posts";
                return RedirectToPage();
            }
            var result = await _postService.UpdatePostAsync(PostId, GetSellerId(), new UpdateSellerPostRequest
            {
                Title = Title.Trim(),
                Content = Content.Trim(),
                Image = Image
            });
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            TempData["ActiveTab"] = "posts";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeletePostAsync(long PostId)
        {
            var result = await _postService.DeletePostAsync(PostId, GetSellerId());
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            TempData["ActiveTab"] = "posts";
            return RedirectToPage();
        }

        // ── Bank Info ────────────────────────────────────────────────────────────
        public async Task<IActionResult> OnPostUpdateBankInfoAsync(
            string? BankBin, string? BankAccountNumber, string? BankAccountName)
        {
            if (string.IsNullOrWhiteSpace(BankBin) || string.IsNullOrWhiteSpace(BankAccountNumber))
            {
                TempData["Error"] = "Mã ngân hàng và số tài khoản là bắt buộc.";
                TempData["ActiveTab"] = "bank";
                return RedirectToPage();
            }
            await _sellerService.UpdateBankInfoAsync(GetSellerId(), BankBin, BankAccountNumber, BankAccountName);
            TempData["Success"] = "Thông tin tài khoản ngân hàng đã được cập nhật.";
            TempData["ActiveTab"] = "bank";
            return RedirectToPage();
        }
    }
}
