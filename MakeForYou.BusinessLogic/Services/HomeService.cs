using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;


namespace MakeForYou.BusinessLogic.Services
{
    public class HomeService : IHomeService
    {
        private readonly IProductRepository _productRepository;

        public HomeService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // Get latest / featured products for homepage
        public async Task<List<Product>> GetFeaturedProductsAsync()
        {
            return await _productRepository.GetFeaturedAsync(10);
        }

        // Get all categories for browse section
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _productRepository.GetCategoriesAsync();
        }

        // Get featured artisans (sellers)
        public async Task<List<Seller>> GetFeaturedArtisansAsync()
        {
            return await _productRepository.GetFeaturedSellersAsync(4);
        }
    }
}
