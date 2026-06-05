using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MakeForYou.BusinessLogic.Services.Interfaces;
using MakeForYou.BusinessLogic.Entities.DTOs.Ghn;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace MakeForYou.Presentation.Pages.Seller
{
    public class GhnConfigModel : PageModel
    {
        private readonly IGhnStoreService _ghnStoreService;
        private readonly ILogger<GhnConfigModel> _logger;

        [BindProperty]
        public int? CurrentGhnShopId { get; set; }

        public GhnConfigModel(IGhnStoreService ghnStoreService, ILogger<GhnConfigModel> logger)
        {
            _ghnStoreService = ghnStoreService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null)
            {
                _logger.LogWarning("Unauthorized access attempt to GHN config page");
                return RedirectToPage("/Account/Login");
            }

            try
            {
                _logger.LogInformation("Loading GHN config for seller {SellerId}", sellerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading GHN config for seller {SellerId}", sellerId);
            }

            return Page();
        }

        // ── LOCATION PROXY HANDLERS ───────────────────────────────────────────
        // These keep the GHN token server-side — JS never sees it.

        /// <summary>GET ?handler=Provinces</summary>
        public async Task<IActionResult> OnGetProvincesAsync()
        {
            try
            {
                var data = await _ghnStoreService.ProxyGetAsync(
                    "/shiip/public-api/master-data/province");
                return new JsonResult(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProxyGetProvinces failed");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>GET ?handler=Districts&amp;provinceId={id}</summary>
        public async Task<IActionResult> OnGetDistrictsAsync(int provinceId)
        {
            try
            {
                var data = await _ghnStoreService.ProxyGetAsync(
                    $"/shiip/public-api/master-data/district?province_id={provinceId}");
                return new JsonResult(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProxyGetDistricts failed for provinceId={ProvinceId}", provinceId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>GET ?handler=Wards&amp;districtId={id}</summary>
        public async Task<IActionResult> OnGetWardsAsync(int districtId)
        {
            try
            {
                var data = await _ghnStoreService.ProxyGetAsync(
                    $"/shiip/public-api/master-data/ward?district_id={districtId}");
                return new JsonResult(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProxyGetWards failed for districtId={DistrictId}", districtId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ── STORE HANDLERS ────────────────────────────────────────────────────

        /// <summary>
        /// [Tab 1] Tạo GHN store mới qua Form.
        /// POST ?handler=CreateStore
        /// </summary>
        public async Task<IActionResult> OnPostCreateStoreAsync(GhnCreateStoreRequest request)
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null)
            {
                _logger.LogWarning("Unauthorized create store attempt");
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for create store: {Errors}",
                    string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))));
                return Page();
            }

            try
            {
                _logger.LogInformation("[CreateStore] Starting for seller {SellerId}", sellerId);
                var result = await _ghnStoreService.CreateStoreAsync(sellerId.Value, request);
                _logger.LogInformation("[CreateStore] Success. ShopId={ShopId}", result.ShopId);
                return RedirectToPage(new { success = true });
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "[CreateStore] HTTP error for seller {SellerId}", sellerId);
                ModelState.AddModelError(string.Empty, $"Lỗi kết nối API GHN: {httpEx.Message}");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CreateStore] Error for seller {SellerId}", sellerId);
                ModelState.AddModelError(string.Empty, $"Lỗi tạo cửa hàng: {ex.GetType().Name} – {ex.Message}");
                return Page();
            }
        }

        /// <summary>
        /// [Tab 2 & 3] Liên kết GHN ShopId có sẵn.
        /// POST ?handler=LinkManual
        /// </summary>
        public async Task<IActionResult> OnPostLinkManualAsync(int ghnShopId)
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null)
            {
                _logger.LogWarning("Unauthorized link manual attempt");
                return Forbid();
            }

            try
            {
                _logger.LogInformation("[LinkManual] seller={SellerId}, ghnShopId={ShopId}",
                    sellerId, ghnShopId);
                await _ghnStoreService.LinkExistingStoreAsync(sellerId.Value, ghnShopId);
                _logger.LogInformation("[LinkManual] Success");
                return RedirectToPage(new { success = true });
            }
            catch (ArgumentException argEx)
            {
                _logger.LogWarning(argEx, "[LinkManual] Invalid argument");
                ModelState.AddModelError(string.Empty, $"Dữ liệu không hợp lệ: {argEx.Message}");
                return Page();
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "[LinkManual] HTTP error");
                ModelState.AddModelError(string.Empty, $"Lỗi kết nối API GHN: {httpEx.Message}");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LinkManual] Error");
                ModelState.AddModelError(string.Empty, $"Lỗi liên kết: {ex.GetType().Name} – {ex.Message}");
                return Page();
            }
        }

        /// <summary>
        /// API endpoint cho Tab 2 (gọi từ JavaScript).
        /// GET ?handler=Stores
        /// </summary>
        public async Task<IActionResult> OnGetStoresAsync()
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null)
            {
                _logger.LogWarning("[GetStores] Unauthorized");
                return new UnauthorizedObjectResult(new { error = "Unauthorized", message = "Bạn cần đăng nhập" });
            }

            try
            {
                _logger.LogInformation("[GetStores] seller={SellerId}", sellerId);
                var stores = await _ghnStoreService.GetStoresAsync(sellerId.Value);
                _logger.LogInformation("[GetStores] Found {Count} stores", stores?.Count ?? 0);
                return new JsonResult(stores);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "[GetStores] HTTP error");
                return BadRequest(new { error = "API_HTTP_ERROR", message = httpEx.Message });
            }
            catch (InvalidOperationException invalidEx)
            {
                _logger.LogError(invalidEx, "[GetStores] Config error");
                return BadRequest(new { error = "CONFIG_ERROR", message = invalidEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetStores] Error");
                return StatusCode(500, new { error = "INTERNAL_ERROR", message = ex.Message });
            }
        }

        // ── HELPERS ───────────────────────────────────────────────────────────

        private long? GetCurrentSellerId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (long.TryParse(idClaim, out var id)) return id;
            _logger.LogWarning("Failed to parse seller ID from claims");
            return null;
        }
    }
}