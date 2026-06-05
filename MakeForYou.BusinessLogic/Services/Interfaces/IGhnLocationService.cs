using MakeForYou.BusinessLogic.DTOs;

namespace MakeForYou.BusinessLogic.Interfaces
{
    public interface IGhnLocationService
    {
        Task<List<GhnProvinceDto>> GetProvincesAsync();
        Task<List<GhnDistrictDto>> GetDistrictsAsync(int provinceId);
        Task<List<GhnWardDto>> GetWardsAsync(int districtId);
    }
}