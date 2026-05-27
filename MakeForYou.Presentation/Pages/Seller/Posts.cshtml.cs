using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MakeForYou.Presentation.Pages.Seller
{
    [Authorize(Roles = "Seller")]
    public class PostsModel : PageModel
    {
        private readonly ISellerPostService _postService;

        public PostsModel(ISellerPostService postService)
        {
            _postService = postService;
        }

        public List<SellerPost> Posts { get; set; } = new();

        private long GetSellerId() =>
            long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<IActionResult> OnGetAsync()
        {
            Posts = await _postService.GetPostsBySellerAsync(GetSellerId());
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync(
            string Title, string Content, IFormFile? Image)
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Content))
            {
                TempData["Error"] = "Title and content are required.";
                return RedirectToPage();
            }

            var result = await _postService.CreatePostAsync(GetSellerId(), new CreateSellerPostRequest
            {
                Title = Title.Trim(),
                Content = Content.Trim(),
                Image = Image
            });

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(
            long PostId, string Title, string Content, IFormFile? Image)
        {
            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Content))
            {
                TempData["Error"] = "Title and content are required.";
                return RedirectToPage();
            }

            var result = await _postService.UpdatePostAsync(PostId, GetSellerId(), new UpdateSellerPostRequest
            {
                Title = Title.Trim(),
                Content = Content.Trim(),
                Image = Image
            });

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(long PostId)
        {
            var result = await _postService.DeletePostAsync(PostId, GetSellerId());
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToPage();
        }
    }
}
