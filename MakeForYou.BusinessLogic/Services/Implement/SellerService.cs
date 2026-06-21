using MakeForYou.BusinessLogic.DTOs;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using MakeForYou.BusinessLogic.ViewModels;
using MakeForYou.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class SellerService : ISellerService
    {
        private readonly ISellerRepository _sellerRepo;
        private readonly IUserRepository _userRepo;
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public SellerService(ISellerRepository sellerRepo, IUserRepository userRepo, ApplicationDbContext dbContext, IWebHostEnvironment env)
        {
            _sellerRepo = sellerRepo;
            _userRepo = userRepo;
            _dbContext = dbContext;
            _env = env;
        }

        public Task<MakeForYou.BusinessLogic.Entities.Seller?> GetProfileAsync(long sellerId) =>
            _sellerRepo.GetWithDetailsAsync(sellerId);

        public async Task UpdateProfileAsync(long sellerId, UpdateProfileRequest request)
        {
            var seller = await _sellerRepo.GetWithDetailsAsync(sellerId);
            if (seller == null) return;

            seller.SkillDescription = request.SkillDescription ?? seller.SkillDescription;
            seller.Bio = request.Bio ?? seller.Bio;
            seller.PriceRange = request.PriceRange ?? seller.PriceRange;
            seller.AvailabilityStatus = request.AvailabilityStatus ?? seller.AvailabilityStatus;

            if (request.Avatar != null && request.Avatar.Length > 0)
            {
                var url = await TrySaveProfileImageAsync(request.Avatar, prefix: $"avatar_{seller.SellerId}_{DateTime.UtcNow:yyyyMMddHHmmss}");
                if (url != null)
                {
                    DeleteImageFile(seller.AvatarUrl);
                    seller.AvatarUrl = url;
                }
            }

            if (request.Cover != null && request.Cover.Length > 0)
            {
                var url = await TrySaveProfileImageAsync(request.Cover, prefix: $"cover_{seller.SellerId}_{DateTime.UtcNow:yyyyMMddHHmmss}");
                if (url != null)
                {
                    DeleteImageFile(seller.CoverImageUrl);
                    seller.CoverImageUrl = url;
                }
            }

            if (seller.User != null)
            {
                if (!string.IsNullOrWhiteSpace(request.FullName))
                    seller.User.FullName = request.FullName;
                if (!string.IsNullOrWhiteSpace(request.Email))
                    seller.User.Email = request.Email;

                await _userRepo.UpdateAsync(seller.User);
            }

            await _sellerRepo.UpdateAsync(seller);
        }

        public async Task<bool> ChangePasswordAsync(long sellerId, string currentPassword, string newPassword)
        {
            var seller = await _sellerRepo.GetWithDetailsAsync(sellerId);
            if (seller?.User == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, seller.User.PasswordHash))
                return false;

            seller.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepo.UpdateAsync(seller.User);
            return true;
        }

        public async Task UpdateBankInfoAsync(long sellerId, string? bankBin, string? bankAccountNumber, string? bankAccountName)
        {
            var seller = await _sellerRepo.GetWithDetailsAsync(sellerId);
            if (seller == null) return;

            seller.BankBin = bankBin?.Trim();
            seller.BankAccountNumber = bankAccountNumber?.Trim();
            seller.BankAccountName = bankAccountName?.Trim();

            await _sellerRepo.UpdateAsync(seller);
        }

        public async Task<bool> IsSellerSetupAsync(string userId)
        {
            if (!long.TryParse(userId, out var id)) return false;
            return await _dbContext.Sellers
                .AnyAsync(s => s.SellerId == id && s.ShopName != null);
        }

        public async Task<ServiceResult> RegisterSellerAsync(string userId, SellerRegisterViewModel model)
        {
            if (!long.TryParse(userId, out var id))
                return new ServiceResult { Success = false, ErrorMessage = "User không hợp lệ" };

            var seller = await _dbContext.Sellers.FindAsync(id);
            if (seller == null)
                return new ServiceResult { Success = false, ErrorMessage = "Bạn chưa đăng ký tài khoản Seller" };

            seller.ShopName = model.ShopName;
            seller.ShopDescription = model.ShopDescription;
            seller.PickupFullName = model.PickupFullName;
            seller.PickupPhone = model.PickupPhone;
            seller.Province = model.Province;
            seller.District = model.District;
            seller.Ward = model.Ward;
            seller.AddressDetail = model.AddressDetail;
            seller.AvailabilityStatus ??= 1;

            if (!string.IsNullOrWhiteSpace(model.BankBin) && !string.IsNullOrWhiteSpace(model.BankAccountNumber))
            {
                seller.BankBin = model.BankBin.Trim();
                seller.BankAccountNumber = model.BankAccountNumber.Trim();
                seller.BankAccountName = model.BankAccountName?.Trim();
            }

            // The Register form no longer collects a separate account phone number — it reuses
            // the pickup contact phone, falling back to PhoneNumber only if a caller still sends it.
            var phoneToSave = !string.IsNullOrWhiteSpace(model.PhoneNumber) ? model.PhoneNumber : model.PickupPhone;

            var user = await _dbContext.Users.FindAsync(id);
            if (user != null && !string.IsNullOrWhiteSpace(phoneToSave))
                user.Phone = ToLocalPhoneFormat(phoneToSave);

            await _dbContext.SaveChangesAsync();
            return new ServiceResult { Success = true };
        }

        // The form submits only the subscriber number (a fixed "+84" prefix is shown next to the
        // input), so re-add the leading 0 to match the local format ("0xxxxxxxxx") used everywhere
        // else in the app (e.g. GhnService's default buyer phone, Auth/Register's phone field).
        private static string ToLocalPhoneFormat(string phone)
        {
            phone = phone.Trim();
            return phone.StartsWith("0") ? phone : "0" + phone;
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        private async Task<string?> TrySaveProfileImageAsync(IFormFile image, string prefix)
        {
            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext)) return null;

            var folder = Path.Combine(_env.WebRootPath, "uploads", "sellers");
            Directory.CreateDirectory(folder);

            var fileName = $"{prefix}{ext}";
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream);

            return $"/uploads/sellers/{fileName}";
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