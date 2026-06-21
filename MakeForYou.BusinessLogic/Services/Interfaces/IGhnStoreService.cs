using MakeForYou.BusinessLogic.Entities.DTOs.Ghn;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    /// <summary>
    /// Abstraction cho các thao tác GHN Store.
    /// </summary>
    public interface IGhnStoreService
    {
        /// <summary>
        /// Tạo một GHN store mới và lưu ShopId vào Seller profile.
        /// </summary>
        Task<GhnCreateStoreResponse> CreateStoreAsync(long sellerId, GhnCreateStoreRequest request);

        /// <summary>
        /// Lấy danh sách stores đang được liên kết với GHN token hiện tại.
        /// sellerId dùng cho logging context.
        /// </summary>
        Task<List<GhnStoreInfo>> GetStoresAsync(long sellerId);

        /// <summary>
        /// Liên kết một GHN ShopId có sẵn vào Seller profile.
        /// </summary>
        Task LinkExistingStoreAsync(long sellerId, int ghnShopId);

        /// <summary>
        /// Proxy một GET request tới GHN master-data API (province/district/ward).
        /// Trả về mảng .data đã unwrap để dùng trực tiếp trong JsonResult.
        /// Token được giữ server-side — client không cần biết.
        /// </summary>
        /// <param name="path">Relative path, e.g. "/shiip/public-api/master-data/province"</param>
        Task<object> ProxyGetAsync(string path);
    }
}