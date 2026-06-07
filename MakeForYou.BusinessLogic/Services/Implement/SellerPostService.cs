using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class SellerPostService : ISellerPostService
    {
        private readonly ISellerPostRepository _postRepo;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public SellerPostService(ISellerPostRepository postRepo, IWebHostEnvironment env)
        {
            _postRepo = postRepo;
            _env = env;
        }

        public Task<List<SellerPost>> GetPostsBySellerAsync(long sellerId) =>
            _postRepo.GetBySellerIdAsync(sellerId);

        public Task<SellerPost?> GetPostByIdAsync(long postId) =>
            _postRepo.GetByIdAsync(postId);

        public async Task<AuthResult> CreatePostAsync(long sellerId, CreateSellerPostRequest req)
        {
            string? imageUrl = null;

            if (req.Image != null && req.Image.Length > 0)
            {
                var saveResult = await TrySaveImageAsync(req.Image, prefix: $"post_{DateTime.UtcNow:yyyyMMddHHmmss}");
                if (!saveResult.Success)
                    return AuthResult.Fail(saveResult.Error!);
                imageUrl = saveResult.Url;
            }

            var post = new SellerPost
            {
                SellerId = sellerId,
                Title = req.Title.Trim(),
                Content = req.Content.Trim(),
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _postRepo.AddAsync(post);
            return AuthResult.Ok("Post created successfully.");
        }

        public async Task<AuthResult> UpdatePostAsync(long postId, long sellerId, UpdateSellerPostRequest req)
        {
            var post = await _postRepo.GetByIdAsync(postId);

            if (post == null)
                return AuthResult.Fail("Post not found.");

            if (post.SellerId != sellerId)
                return AuthResult.Fail("Access denied.");

            post.Title = req.Title.Trim();
            post.Content = req.Content.Trim();
            post.UpdatedAt = DateTime.UtcNow;

            if (req.Image != null && req.Image.Length > 0)
            {
                var saveResult = await TrySaveImageAsync(req.Image, prefix: $"{postId}_{DateTime.UtcNow:yyyyMMddHHmmss}");
                if (!saveResult.Success)
                    return AuthResult.Fail(saveResult.Error!);

                // Remove the old image file from disk
                DeleteImageFile(post.ImageUrl);

                post.ImageUrl = saveResult.Url;
            }

            await _postRepo.UpdateAsync(post);
            return AuthResult.Ok("Post updated successfully.");
        }

        public async Task<AuthResult> DeletePostAsync(long postId, long sellerId)
        {
            var post = await _postRepo.GetByIdAsync(postId);

            if (post == null)
                return AuthResult.Fail("Post not found.");

            if (post.SellerId != sellerId)
                return AuthResult.Fail("Access denied.");

            DeleteImageFile(post.ImageUrl);

            var deleted = await _postRepo.DeleteAsync(postId, sellerId);
            return deleted
                ? AuthResult.Ok("Post deleted.")
                : AuthResult.Fail("Could not delete post.");
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        private async Task<(bool Success, string? Url, string? Error)> TrySaveImageAsync(
            IFormFile image, string prefix)
        {
            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
                return (false, null, "Only JPG, PNG, or WEBP images are allowed.");

            var folder = Path.Combine(_env.WebRootPath, "uploads", "posts");
            Directory.CreateDirectory(folder);

            var fileName = $"{prefix}{ext}";
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream);

            return (true, $"/uploads/posts/{fileName}", null);
        }

        private void DeleteImageFile(string? relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return;
            try
            {
                var physicalPath = Path.Combine(_env.WebRootPath, relativeUrl.TrimStart('/'));
                if (File.Exists(physicalPath))
                    File.Delete(physicalPath);
            }
            catch
            {
                // Non-critical — file may have already been removed
            }
        }
    }
}
