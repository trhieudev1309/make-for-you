using MakeForYou.BusinessLogic.DTOs;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface ISellerService
    {
        Task<MakeForYou.BusinessLogic.Entities.Seller?> GetProfileAsync(long sellerId);
        Task UpdateProfileAsync(long sellerId, UpdateProfileRequest request);
        Task<bool> ChangePasswordAsync(long sellerId, string currentPassword, string newPassword);
    }
}