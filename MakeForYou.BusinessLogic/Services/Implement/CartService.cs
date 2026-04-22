using System.Text.Json;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Interfaces; // Gom tất cả Interface vào đây
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepo;
        private readonly IProductRepository _productRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CART_SESSION_KEY = "GuestCart";

        public CartService(
            ICartRepository cartRepo,
            IProductRepository productRepo,
            IHttpContextAccessor httpContextAccessor)
        {
            _cartRepo = cartRepo;
            _productRepo = productRepo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task AddToCartAsync(long? userId, long productId, int quantity)
        {
            if (userId.HasValue) // --- TRƯỜNG HỢP ĐÃ ĐĂNG NHẬP (Lưu Database) ---
            {
                var existing = await _cartRepo.GetExistingItemAsync(userId.Value, productId);
                if (existing != null)
                {
                    existing.Quantity += quantity;
                    await _cartRepo.UpdateItemAsync(existing);
                }
                else
                {
                    // Lấy thông tin sản phẩm để chốt giá lúc thêm vào
                    var product = await _productRepo.FindByIdAsync(productId);

                    await _cartRepo.AddItemAsync(new CartItem
                    {
                        UserId = userId,
                        ProductId = productId,
                        Quantity = quantity,
                        PriceAtAdd = product?.Price ?? 0
                    });
                }
            }
            else // --- TRƯỜNG HỢP KHÁCH VÃNG LAI (Lưu Session) ---
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                var cartJson = session?.GetString(CART_SESSION_KEY);

                List<CartItem> cart = string.IsNullOrEmpty(cartJson)
                    ? new List<CartItem>()
                    : JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();

                var item = cart.FirstOrDefault(x => x.ProductId == productId);
                if (item != null)
                {
                    item.Quantity += quantity;
                }
                else
                {
                    cart.Add(new CartItem { ProductId = productId, Quantity = quantity });
                }

                session?.SetString(CART_SESSION_KEY, JsonSerializer.Serialize(cart));
            }
        }

        public async Task<int> GetTotalItemsCountAsync(long? userId)
        {
            if (userId.HasValue)
            {
                return await _cartRepo.GetCountByUserIdAsync(userId.Value);
            }

            var session = _httpContextAccessor.HttpContext?.Session;
            var cartJson = session?.GetString(CART_SESSION_KEY);

            if (string.IsNullOrEmpty(cartJson)) return 0;

            var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
            return cart?.Sum(x => x.Quantity) ?? 0;
        }

        public async Task<List<CartItemViewModel>> GetCartAsync(long? userId)
        {
            List<CartItem> rawItems;
            if (userId.HasValue)
            {
                rawItems = await _cartRepo.GetItemsByUserIdAsync(userId.Value);
            }
            else
            {
                var json = _httpContextAccessor.HttpContext?.Session.GetString(CART_SESSION_KEY);
                rawItems = string.IsNullOrEmpty(json) ? new List<CartItem>()
                           : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();

                // Khách vãng lai: Trong Session chỉ có ProductId, cần tìm thêm info từ ProductRepo
                foreach (var item in rawItems)
                {
                    item.Product = await _productRepo.FindByIdAsync(item.ProductId);
                }
            }

            return rawItems.Select(x => new CartItemViewModel
            {
                ProductId = x.ProductId,
                ProductName = x.Product?.Title,
                ImageUrl = x.Product?.ImageUrl,
                Price = x.Product?.Price ?? 0,
                Quantity = x.Quantity
            }).ToList();
        }

        public async Task UpdateQuantityAsync(long? userId, long productId, int quantity)
        {
            if (quantity <= 0)
            {
                await RemoveItemAsync(userId, productId);
                return;
            }

            if (userId.HasValue) // Update Database
            {
                var item = await _cartRepo.GetExistingItemAsync(userId.Value, productId);
                if (item != null)
                {
                    item.Quantity = quantity;
                    await _cartRepo.UpdateItemAsync(item);
                }
            }
            else // Update Session
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                var cartJson = session?.GetString(CART_SESSION_KEY);
                if (!string.IsNullOrEmpty(cartJson))
                {
                    var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
                    var item = cart?.FirstOrDefault(x => x.ProductId == productId);
                    if (item != null)
                    {
                        item.Quantity = quantity;
                        session?.SetString(CART_SESSION_KEY, JsonSerializer.Serialize(cart));
                    }
                }
            }
        }

        public async Task RemoveItemAsync(long? userId, long productId)
        {
            if (userId.HasValue) // Remove from Database
            {
                var item = await _cartRepo.GetExistingItemAsync(userId.Value, productId);
                if (item != null)
                {
                    await _cartRepo.DeleteItemAsync(item);
                }
            }
            else // Remove from Session
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                var cartJson = session?.GetString(CART_SESSION_KEY);
                if (!string.IsNullOrEmpty(cartJson))
                {
                    var cart = JsonSerializer.Deserialize<List<CartItem>>(cartJson);
                    var item = cart?.FirstOrDefault(x => x.ProductId == productId);
                    if (item != null)
                    {
                        cart!.Remove(item);
                        session?.SetString(CART_SESSION_KEY, JsonSerializer.Serialize(cart));
                    }
                }
            }
        }
    }
}