using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Services
{
    public interface IGhnService
    {
        /// <summary>
        /// Tính toán tổng chi phí vận chuyển từ API Giao Hàng Nhanh dựa trên giỏ hàng và địa chỉ nhận
        /// </summary>
        Task<int> CalculateShippingFeeAsync(List<Product> cartItems, int toDistrictId, string toWardCode);

        /// <summary>
        /// Lấy gói dịch vụ khả dụng từ GHN
        /// </summary>
        Task<List<GhnServiceDto>> GetAvailableServicesAsync(int fromDistrictId, int toDistrictId);

        /// <summary>
        /// Lấy chi tiết phí dịch vụ của đơn hàng đã tạo bằng mã đơn hàng (soc)
        /// </summary>
        Task<string> GetOrderDetailAsync(string orderCode);
    }
}