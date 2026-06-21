using MakeForYou.BusinessLogic.ViewModels;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MakeForYou.BusinessLogic.Entities.DTOs.Ghn;

[ApiController]
[Route("api/seller")]
[Authorize]
public class SellerController : ControllerBase
{
    private readonly ISellerService _sellerService;
    private readonly IGhnStoreService _ghnStoreService;
    private readonly ILogger<SellerController> _logger;

    public SellerController(ISellerService sellerService, IGhnStoreService ghnStoreService, ILogger<SellerController> logger)
    {
        _sellerService = sellerService;
        _ghnStoreService = ghnStoreService;
        _logger = logger;
    }

    // GET /api/seller/check
    [HttpGet("check")]
    public async Task<IActionResult> Check()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isSetup = await _sellerService.IsSellerSetupAsync(userId);
        return Ok(new { isSetup });
    }

    // POST /api/seller/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromForm] SellerRegisterViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (!User.IsInRole("Seller"))
            return Forbid();

        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var wasAlreadySetup = await _sellerService.IsSellerSetupAsync(userIdStr);

        var result = await _sellerService.RegisterSellerAsync(userIdStr, model);

        if (result.Success)
        {
            // Editing an already-registered shop: just save the changes, don't re-create the GHN store.
            if (wasAlreadySetup)
                return Ok(new { message = "Cập nhật thông tin gian hàng thành công!" });

            if (long.TryParse(userIdStr, out var sellerId))
            {
                try
                {
                    // Attempt to create GHN store
                    var ghnRequest = new GhnCreateStoreRequest
                    {
                        Name = model.ShopName,
                        Phone = model.PickupPhone, // Using PickupPhone or PhoneNumber as required
                        Address = model.AddressDetail,
                        WardCode = model.WardCode,
                        DistrictId = model.DistrictId,
                    };
                    await _ghnStoreService.CreateStoreAsync(sellerId, ghnRequest);

                    return Ok(new { message = "Đăng ký gian hàng và liên kết GHN thành công!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating GHN store during registration for seller {SellerId}", sellerId);
                    // Option B: Allow registration to succeed but indicate GHN config failed
                    return Ok(new { message = "Đăng ký gian hàng thành công! (Lưu ý: Chưa thể kết nối GHN tự động, vui lòng thử lại trong trang Hồ sơ)." });
                }
            }
            return Ok(new { message = "Đăng ký gian hàng thành công!" });
        }

        return BadRequest(new { error = result.ErrorMessage });
    }

    // ── LOCATION PROXY HANDLERS ───────────────────────────────────────────

    [HttpGet("ghn/provinces")]
    public async Task<IActionResult> GetProvinces()
    {
        try
        {
            var data = await _ghnStoreService.ProxyGetAsync("/shiip/public-api/master-data/province");
            return new JsonResult(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProxyGetProvinces failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("ghn/districts")]
    public async Task<IActionResult> GetDistricts([FromQuery] int provinceId)
    {
        try
        {
            var data = await _ghnStoreService.ProxyGetAsync($"/shiip/public-api/master-data/district?province_id={provinceId}");
            return new JsonResult(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProxyGetDistricts failed for provinceId={ProvinceId}", provinceId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("ghn/wards")]
    public async Task<IActionResult> GetWards([FromQuery] int districtId)
    {
        try
        {
            var data = await _ghnStoreService.ProxyGetAsync($"/shiip/public-api/master-data/ward?district_id={districtId}");
            return new JsonResult(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProxyGetWards failed for districtId={DistrictId}", districtId);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}