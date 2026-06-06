using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MakeForYou.BusinessLogic.Entities.DTOs.Ghn;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class GhnStoreService : IGhnStoreService
    {
        private const string GhnBaseUrl = "https://dev-online-gateway.ghn.vn";

        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<GhnStoreService> _logger;

        public GhnStoreService(
            IHttpClientFactory httpClientFactory,
            ApplicationDbContext db,
            IConfiguration configuration,
            ILogger<GhnStoreService> logger)
        {
            _db = db;
            _logger = logger;

            _httpClient = httpClientFactory.CreateClient("GHN");

            var token = configuration["Ghn:Token"]
                ?? throw new InvalidOperationException("Ghn:Token chưa được cấu hình trong appsettings.");

            _httpClient.BaseAddress = new Uri(GhnBaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Token", token);
        }

        /// <inheritdoc />
        public async Task<GhnCreateStoreResponse> CreateStoreAsync(
            long sellerId, GhnCreateStoreRequest request)
        {
            _logger.LogInformation("Creating GHN store for SellerId={SellerId}, StoreName={Name}",
                sellerId, request.Name);

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/shiip/public-api/v2/shop/register", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GHN CreateStore failed. Status={Status}, Body={Body}",
                    response.StatusCode, responseBody);
                throw new HttpRequestException(
                    $"GHN API trả về lỗi {(int)response.StatusCode}: {responseBody}");
            }

            var apiResult = JsonSerializer.Deserialize<GhnApiResponse<GhnCreateStoreResponse>>(responseBody);

            if (apiResult?.Code != 200 || apiResult.Data == null)
            {
                _logger.LogError("GHN CreateStore unsuccessful. Code={Code}, Message={Message}",
                    apiResult?.Code, apiResult?.Message);
                throw new InvalidOperationException(
                    $"GHN báo lỗi: {apiResult?.Message ?? "Unknown error"}");
            }

            await SaveGhnShopIdAsync(sellerId, apiResult.Data.ShopId);

            _logger.LogInformation("GHN store created. ShopId={ShopId}", apiResult.Data.ShopId);
            return apiResult.Data;
        }

        /// <inheritdoc />
        public async Task<List<GhnStoreInfo>> GetStoresAsync(long sellerId)
        {
            _logger.LogInformation("[GetStores] Fetching stores for seller {SellerId}", sellerId);

            var response = await _httpClient.GetAsync("/shiip/public-api/v2/shop/all");
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GHN GetStores failed. Status={Status}, Body={Body}",
                    response.StatusCode, responseBody);
                throw new HttpRequestException(
                    $"GHN API trả về lỗi {(int)response.StatusCode}: {responseBody}");
            }

            var apiResult = JsonSerializer.Deserialize<GhnApiResponse<GhnGetStoreResponse>>(responseBody);

            if (apiResult?.Code != 200 || apiResult.Data == null)
            {
                _logger.LogError("GHN GetStores unsuccessful. Code={Code}, Message={Message}",
                    apiResult?.Code, apiResult?.Message);
                throw new InvalidOperationException(
                    $"GHN báo lỗi: {apiResult?.Message ?? "Unknown error"}");
            }

            return apiResult.Data.Shops ?? new List<GhnStoreInfo>();
        }

        /// <inheritdoc />
        public async Task LinkExistingStoreAsync(long sellerId, int ghnShopId)
        {
            _logger.LogInformation("Linking GHN ShopId={ShopId} to SellerId={SellerId}",
                ghnShopId, sellerId);

            var stores = await GetStoresAsync(sellerId);
            var exists = stores.Any(s => s.Id == ghnShopId);

            if (!exists)
                throw new ArgumentException(
                    $"GHN ShopId={ghnShopId} không tồn tại trong tài khoản GHN.");

            await SaveGhnShopIdAsync(sellerId, ghnShopId);
        }

        /// <inheritdoc />
        /// <summary>
        /// Proxy một GET request tới GHN master-data API và trả về mảng .data đã unwrap.
        /// Dùng cho các endpoint tỉnh/huyện/xã — token được giữ server-side.
        /// </summary>
        public async Task<object> ProxyGetAsync(string path)
        {
            var response = await _httpClient.GetAsync(path);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("GHN ProxyGet failed. Path={Path}, Status={Status}, Body={Body}",
                    path, response.StatusCode, responseBody);
                throw new HttpRequestException(
                    $"GHN API trả về lỗi {(int)response.StatusCode}: {responseBody}");
            }

            var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (root.GetProperty("code").GetInt32() != 200)
            {
                var msg = root.TryGetProperty("message", out var m) ? m.GetString() : "Unknown error";
                _logger.LogError("GHN ProxyGet unsuccessful. Path={Path}, Message={Message}", path, msg);
                throw new InvalidOperationException($"GHN báo lỗi: {msg}");
            }

            // Return the raw JsonElement — JsonResult will serialize it correctly
            return root.GetProperty("data");
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private async Task SaveGhnShopIdAsync(long sellerId, int ghnShopId)
        {
            var seller = await _db.Sellers.FindAsync(sellerId)
                ?? throw new KeyNotFoundException($"Seller với ID={sellerId} không tìm thấy.");

            seller.GhnShopId = ghnShopId;
            await _db.SaveChangesAsync();
        }
    }
}