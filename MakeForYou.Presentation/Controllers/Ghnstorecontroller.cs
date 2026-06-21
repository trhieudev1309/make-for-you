using MakeForYou.BusinessLogic.Entities.DTOs.Ghn;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MakeForYou.BusinessLogic.Controllers
{
    [ApiController]
    [Route("api/ghn/stores")]
    [Authorize] // Cần đăng nhập; phân quyền chi tiết ở từng action
    public class GhnStoreController : ControllerBase
    {
        private readonly IGhnStoreService _ghnStoreService;

        public GhnStoreController(IGhnStoreService ghnStoreService)
        {
            _ghnStoreService = ghnStoreService;
        }

        /// <summary>
        /// [Seller / Admin] Tạo GHN store mới và liên kết với seller profile.
        /// POST /api/ghn/stores/register
        /// </summary>
        [HttpPost("register")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> CreateStore([FromBody] GhnCreateStoreRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var sellerId = GetCurrentSellerId();
            if (sellerId == null)
                return Forbid();

            var result = await _ghnStoreService.CreateStoreAsync(sellerId.Value, request);
            return Ok(new
            {
                message = "Tạo GHN store thành công.",
                shopId = result.ShopId,
                storeName = result.Name
            });
        }

        /// <summary>
        /// [Seller / Admin] Lấy danh sách tất cả stores của tài khoản GHN.
        /// GET /api/ghn/stores
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> GetStores()
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null)
                return Forbid();

            var stores = await _ghnStoreService.GetStoresAsync(sellerId.Value); // ✅
            return Ok(stores);
        }

        /// <summary>
        /// [Seller / Admin] Liên kết một GHN ShopId có sẵn vào seller profile.
        /// POST /api/ghn/stores/link
        /// Body: { "ghnShopId": 12345 }
        /// </summary>
        [HttpPost("link")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> LinkStore([FromBody] LinkStoreRequest request)
        {
            var sellerId = GetCurrentSellerId();
            if (sellerId == null)
                return Forbid();

            await _ghnStoreService.LinkExistingStoreAsync(sellerId.Value, request.GhnShopId);
            return Ok(new { message = $"Đã liên kết GHN ShopId={request.GhnShopId} thành công." });
        }

        /// <summary>
        /// [Admin only] Liên kết GHN ShopId cho bất kỳ seller nào.
        /// POST /api/ghn/stores/admin/link/{sellerId}
        /// </summary>
        [HttpPost("admin/link/{sellerId:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminLinkStore(long sellerId, [FromBody] LinkStoreRequest request)
        {
            await _ghnStoreService.LinkExistingStoreAsync(sellerId, request.GhnShopId);
            return Ok(new { message = $"Admin đã liên kết GHN ShopId={request.GhnShopId} cho SellerId={sellerId}." });
        }

        // ─── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Lấy SellerId từ JWT claims của user hiện tại.
        /// Giả định SellerId == UserId (do [Key, ForeignKey(User)]).
        /// </summary>
        private long? GetCurrentSellerId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (long.TryParse(idClaim, out var id))
                return id;
            return null;
        }
    }

    /// <summary>Request body cho endpoint link store.</summary>
    public class LinkStoreRequest
    {
        public int GhnShopId { get; set; }
    }
}