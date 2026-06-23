using System.Security.Claims;
using System.Text.Json;
using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.Presentation.Pages.Seller
{
    [Authorize(Roles = "Seller")]
    public class SellerEditProductModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICustomizationService _customizationService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;

        public SellerEditProductModel(
            IProductService productService,
            ICustomizationService customizationService,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext context)
        {
            _productService = productService;
            _customizationService = customizationService;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        [BindProperty] public ProductRequest Product { get; set; } = new();
        [BindProperty] public long ProductId { get; set; }
        [BindProperty] public string? CustomizationsJson { get; set; }
        [BindProperty] public IFormFile? UploadedImage { get; set; }
        [BindProperty] public List<IFormFile>? NewImages { get; set; }
        [BindProperty] public string? ImagesJson { get; set; }

        public List<SelectListItem> CategoryOptions { get; set; } = new();
        public List<CustomizationGroupViewModel> ExistingCustomizations { get; set; } = new();
        public string? CurrentImageUrl { get; set; }
        public List<ProductImage> CurrentImages { get; set; } = new();

        private long GetSellerId() => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<IActionResult> OnGetAsync(long id)
        {
            var sellerId = GetSellerId();
            var p = await _context.Products.FindAsync(id);
            if (p == null || p.SellerId != sellerId)
                return RedirectToPage("/Seller/Profile");

            ProductId = id;
            CurrentImageUrl = p.ImageUrl;
            Product = new ProductRequest
            {
                Title = p.Title,
                Description = p.Description ?? string.Empty,
                Price = p.Price ?? 0,
                ImageUrl = p.ImageUrl ?? string.Empty,
                CategoryId = p.CategoryId ?? 0,
                SellerId = sellerId,
                Status = p.Status,
                Weight = p.Weight,
                Length = p.Length,
                Width = p.Width,
                Height = p.Height
            };

            await LoadCategoriesAsync();
            ExistingCustomizations = await _customizationService.GetCustomizationViewModelsByProductIdAsync(id);
            CurrentImages = await _context.ProductImages.Where(pi => pi.ProductId == id).OrderBy(pi => pi.DisplayOrder).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var sellerId = GetSellerId();
            var existing = await _context.Products.FindAsync(ProductId);
            if (existing == null || existing.SellerId != sellerId)
                return RedirectToPage("/Seller/Profile");

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                ExistingCustomizations = await _customizationService.GetCustomizationViewModelsByProductIdAsync(ProductId);
                CurrentImageUrl = existing.ImageUrl;
                return Page();
            }

            Product.SellerId = sellerId;
            // Handle image gallery operations (add/delete/reorder)
            // ImagesJson carries client-side actions; NewImages contains uploaded files for new entries
            var imagesPayload = new List<ImagePayload>();
            if (!string.IsNullOrWhiteSpace(ImagesJson))
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    imagesPayload =
                       JsonSerializer.Deserialize<List<ImagePayload>>(ImagesJson!, options);
                }
                catch
                {
                    imagesPayload = new();
                }
            }

            // Begin transaction to ensure consistency when updating images + product
            using (var tx = await _context.Database.BeginTransactionAsync())
            {
                // load current images
                var existingImages = await _context.ProductImages.Where(pi => pi.ProductId == ProductId).ToListAsync();

                // Process deletions and updates
                foreach (var pImg in imagesPayload.Where(p => p.Id > 0))
                {
                    var ent = existingImages.FirstOrDefault(x => x.ProductImageId == pImg.Id);
                    if (ent == null) continue;
                    if (pImg.Deleted)
                    {
                        // delete file
                        try
                        {
                            if (!string.IsNullOrEmpty(ent.Url) && ent.Url.StartsWith("/"))
                            {
                                var phys = Path.Combine(_webHostEnvironment.WebRootPath, ent.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                                if (System.IO.File.Exists(phys)) System.IO.File.Delete(phys);
                            }
                        }
                        catch { }
                        _context.ProductImages.Remove(ent);
                    }
                    else
                    {
                        ent.DisplayOrder = pImg.DisplayOrder;
                        ent.IsPrimary = pImg.IsPrimary;
                        _context.ProductImages.Update(ent);
                    }
                }

                // Process new uploads — use Request.Form.Files to robustly access posted files
                var newImagesPayload = imagesPayload.Where(p => p.Id == 0 && !p.Deleted).ToList();
                if (newImagesPayload.Any())
                {
                    var uploadedFiles = Request.Form.Files;
                    string folder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
                    Directory.CreateDirectory(folder);

                    // Process files sequentially by index
                    for (int fileIdx = 0; fileIdx < uploadedFiles.Count; fileIdx++)
                    {
                        var file = uploadedFiles[fileIdx];
                        if (file != null && file.Length > 0)
                        {
                            // Find corresponding image payload entry
                            var newImg = newImagesPayload.FirstOrDefault(p => p.FileIndex == fileIdx);
                            if (newImg != null)
                            {
                                var fileName = Guid.NewGuid() + "_" + Path.GetFileName(file.FileName);
                                var phys = Path.Combine(folder, fileName);
                                using var fs = new FileStream(phys, FileMode.Create);
                                await file.CopyToAsync(fs);
                                var url = "/uploads/products/" + fileName;
                                var ent = new ProductImage { ProductId = ProductId, Url = url, DisplayOrder = newImg.DisplayOrder, IsPrimary = newImg.IsPrimary };
                                _context.ProductImages.Add(ent);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                // Ensure one primary exists
                var allImages = await _context.ProductImages.Where(pi => pi.ProductId == ProductId).OrderBy(pi => pi.DisplayOrder).ToListAsync();
                if (!allImages.Any(i => i.IsPrimary) && allImages.Any())
                {
                    allImages.First().IsPrimary = true;
                    _context.ProductImages.Update(allImages.First());
                    await _context.SaveChangesAsync();
                }

                var primary = allImages.FirstOrDefault(i => i.IsPrimary) ?? allImages.FirstOrDefault();
                Product.ImageUrl = primary?.Url ?? existing.ImageUrl ?? string.Empty;

                // Update product with new ImageUrl
                var ok = await _productService.UpdateProductAsync(ProductId, Product);
                if (!ok)
                {
                    await tx.RollbackAsync();
                    ModelState.AddModelError("", "Lỗi khi cập nhật sản phẩm.");
                    await LoadCategoriesAsync();
                    ExistingCustomizations = await _customizationService.GetCustomizationViewModelsByProductIdAsync(ProductId);
                    CurrentImageUrl = existing.ImageUrl;
                    CurrentImages = await _context.ProductImages.Where(pi => pi.ProductId == ProductId).OrderBy(pi => pi.DisplayOrder).ToListAsync();
                    return Page();
                }

                await tx.CommitAsync();
            }

            await _customizationService.DeleteAllCustomizationsByProductIdAsync(ProductId);
            if (!string.IsNullOrWhiteSpace(CustomizationsJson))
            {
                try
                {
                    var groups = JsonSerializer.Deserialize<List<CustomizationGroupRequest>>(CustomizationsJson);
                    if (groups != null && groups.Count > 0)
                        await _customizationService.CreateCustomizationGroupsAsync(ProductId, groups);
                }
                catch { /* ignore */ }
            }

            TempData["Success"] = "Cập nhật sản phẩm thành công.";
            return RedirectToPage("/Seller/Profile");
        }

        private async Task LoadCategoriesAsync()
        {
            CategoryOptions = await _context.Categories
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToListAsync();
        }

        private class ImagePayload
        {
            public long Id { get; set; }
            public bool Deleted { get; set; }
            public int DisplayOrder { get; set; }
            public bool IsPrimary { get; set; }
            public int? FileIndex { get; set; }
        }
    }
}
