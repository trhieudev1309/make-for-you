using MakeForYou.BusinessLogic.DTOs;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using MakeForYou.BusinessLogic.ViewModels;
using MakeForYou.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class SellerService : ISellerService
    {
        private readonly ISellerRepository _sellerRepo;
        private readonly IUserRepository _userRepo;
        private readonly ApplicationDbContext _dbContext;

        public SellerService(ISellerRepository sellerRepo, IUserRepository userRepo, ApplicationDbContext dbContext)
        {
            _sellerRepo = sellerRepo;
            _userRepo = userRepo;
            _dbContext = dbContext;
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

            if (seller.ShopName != null)
                return new ServiceResult { Success = false, ErrorMessage = "Bạn đã hoàn tất đăng ký gian hàng rồi" };

            seller.ShopName = model.ShopName;
            seller.ShopDescription = model.ShopDescription;
            seller.PickupFullName = model.PickupFullName;
            seller.PickupPhone = model.PickupPhone;
            seller.Province = model.Province;
            seller.District = model.District;
            seller.Ward = model.Ward;
            seller.AddressDetail = model.AddressDetail;
            seller.AvailabilityStatus = 1;

            var user = await _dbContext.Users.FindAsync(id);
            if (user != null && string.IsNullOrEmpty(user.Phone))
                user.Phone = model.PhoneNumber;

            await _dbContext.SaveChangesAsync();
            return new ServiceResult { Success = true };
        }
    }
}