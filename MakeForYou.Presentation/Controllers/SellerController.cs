using MakeForYou.BusinessLogic.ViewModels;
using MakeForYou.BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/seller")]
[Authorize]
public class SellerController : ControllerBase
{
    private readonly ISellerService _sellerService;

    public SellerController(ISellerService sellerService)
    {
        _sellerService = sellerService;
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
    public async Task<IActionResult> Register([FromBody] SellerRegisterViewModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (!User.IsInRole("Seller"))
            return Forbid();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _sellerService.RegisterSellerAsync(userId, model);

        if (result.Success)
            return Ok(new { message = "Đăng ký gian hàng thành công!" });

        return BadRequest(new { error = result.ErrorMessage });
    }
}