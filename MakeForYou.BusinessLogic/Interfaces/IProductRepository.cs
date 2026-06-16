using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Interfaces
{
    public interface IProductRepository
    {
        // Tên hàm phải có chữ "Async" ở cuối và trả về Task
        Task<List<Product>> GetFeaturedAsync(int count);

        Task<List<Category>> GetCategoriesAsync();

        Task<List<Seller>> GetFeaturedSellersAsync(int count);

        Task<Product?> FindByIdAsync(long id); // Tìm sản phẩm theo ID cho trang Details
        Task<List<Product>> SearchAsync(string? searchTerm, long? categoryId); // Tìm kiếm cho trang Index

        Task<Seller?> GetSellerDetailsAsync(long id);

        // Trả về danh sách sản phẩm liên quan (cùng danh mục > cùng nghệ nhân > gần đây)
        Task<List<Product>> GetRelatedProductsAsync(long productId, int count);

        // Trả về các nhận xét (kèm ảnh) của một sản phẩm, mới nhất trước
        Task<List<Review>> GetProductReviewsAsync(long productId, int take);

        // Đếm số lần được đặt hàng cho mỗi sản phẩm (dùng cho trang danh sách)
        Task<Dictionary<long, int>> GetSoldCountsAsync(IEnumerable<long> productIds);

        // Đếm số lần được đặt hàng cho một sản phẩm (dùng cho trang chi tiết)
        Task<int> GetSoldCountAsync(long productId);
    }
}