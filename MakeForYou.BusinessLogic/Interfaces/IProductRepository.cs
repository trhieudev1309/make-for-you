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
    }
}