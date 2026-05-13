using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface IProductService
    {
        // Dành cho Buyer/Home
        Task<List<ProductViewModel>> GetFeaturedProductsAsync();

        // Dành cho Admin (Task 27)

        Task<List<ProductViewModel>> GetAllProductsForAdminAsync();
        Task<bool> CreateProductAsync(ProductRequest req);
        Task<bool> UpdateProductAsync(long id, ProductRequest req);
        Task<bool> DeleteProductAsync(long id); // Thường là Soft Delete (ẩn đi)
    }
}
