using MakeForYou.BusinessLogic.DTOs;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;
using MakeForYou.Repositories.Interfaces;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class SellerService : ISellerService
    {
        private readonly ISellerRepository _sellerRepo;
        private readonly IUserRepository _userRepo;

        public SellerService(ISellerRepository sellerRepo, IUserRepository userRepo)
        {
            _sellerRepo = sellerRepo;
            _userRepo = userRepo;
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
    }
}