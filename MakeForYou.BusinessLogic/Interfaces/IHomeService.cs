using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Interfaces
{
    public interface IHomeService
    {
        Task<List<Product>> GetFeaturedProductsAsync();
        Task<List<Category>> GetCategoriesAsync();
        Task<List<Seller>> GetFeaturedArtisansAsync();
    }
}