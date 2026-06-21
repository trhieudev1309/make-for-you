using Microsoft.AspNetCore.Mvc;
using MakeForYou.BusinessLogic.Interfaces;
using System.Text.Json; // Thêm thư viện này ở đầu file

namespace MakeForYou.Presentation.Controllers
{
    [ApiController]
    [Route("api/ghn")]
    public class GhnLocationController : ControllerBase
    {
        private readonly IGhnLocationService _ghnLocationService;

        // Tạo option ép hệ thống giữ nguyên định dạng của DTO khi nhả về Client
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null // Giữ nguyên chữ hoa thường như cấu trúc Class C#
        };

        public GhnLocationController(IGhnLocationService ghnLocationService)
        {
            _ghnLocationService = ghnLocationService;
        }

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            try
            {
                var provinces = await _ghnLocationService.GetProvincesAsync();
                return JsonSerializer(provinces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("districts")]
        public async Task<IActionResult> GetDistricts([FromQuery] int provinceId)
        {
            if (provinceId <= 0) return BadRequest("Invalid ProvinceId");
            try
            {
                var districts = await _ghnLocationService.GetDistrictsAsync(provinceId);
                return JsonSerializer(districts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("wards")]
        public async Task<IActionResult> GetWards([FromQuery] int districtId)
        {
            if (districtId <= 0) return BadRequest("Invalid DistrictId");
            try
            {
                var wards = await _ghnLocationService.GetWardsAsync(districtId);
                return JsonSerializer(wards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private IActionResult JsonSerializer(object data)
        {
            return new ContentResult
            {
                Content = System.Text.Json.JsonSerializer.Serialize(data, _jsonOptions),
                ContentType = "application/json",
                StatusCode = 200
            };
        }
    }
}