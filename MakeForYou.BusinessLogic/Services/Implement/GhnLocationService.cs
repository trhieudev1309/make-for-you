using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using MakeForYou.BusinessLogic.DTOs;
using MakeForYou.BusinessLogic.Interfaces;

namespace MakeForYou.BusinessLogic.Implement
{
    public class GhnLocationService : IGhnLocationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;

        public GhnLocationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;

            // Đọc cấu hình bảo mật từ appsettings.json đã thiết lập ở Bước 1
            _token = configuration["GHN:Token"] ?? throw new ArgumentNullException("GHN Token is missing");
            var baseUrl = configuration["GHN:BaseUrl"] ?? "https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/";

            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Inject Token vào Header bắt buộc của hệ thống GHN công khai
            _httpClient.DefaultRequestHeaders.Add("Token", _token);
        }

        public async Task<List<GhnProvinceDto>> GetProvincesAsync()
        {
            // 1. Tạo request tường minh tới endpoint province
            var request = new HttpRequestMessage(HttpMethod.Get, "province");

            // 2. Ép trực tiếp Header Token (Đảm bảo chữ T viết hoa)
            request.Headers.Add("Token", _token);

            // 3. Thực thi gọi mạng
            var responseMessage = await _httpClient.SendAsync(request);

            if (responseMessage.IsSuccessStatusCode)
            {
                var response = await responseMessage.Content.ReadFromJsonAsync<GhnApiResponse<List<GhnProvinceDto>>>();
                if (response == null)
                {
                    throw new Exception("Failed to deserialize response from GHN API.");
                }
                if (response.Code != 200)
                {
                    throw new Exception($"GHN API returned error code {response.Code}: {response.Message}");
                }
                return response.Data;
            }
            else
            {
                // Đặt Breakpoint tại đây để xem nội dung báo lỗi thực tế từ GHN nhả về
                var errorContent = await responseMessage.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[GHN Error]: {errorContent}");
                throw new HttpRequestException($"GHN API request failed with status code {responseMessage.StatusCode}. Response: {errorContent}");
            }
        }

        public async Task<List<GhnDistrictDto>> GetDistrictsAsync(int provinceId)
        {
            // Theo tài liệu cURL: Lấy Huyện yêu cầu POST dữ liệu json chứa "province_id"
            var requestPayload = new { province_id = provinceId };
            var responseMessage = await _httpClient.PostAsJsonAsync("district", requestPayload);

            if (responseMessage.IsSuccessStatusCode)
            {
                var response = await responseMessage.Content.ReadFromJsonAsync<GhnApiResponse<List<GhnDistrictDto>>>();
                if (response == null)
                {
                    throw new Exception("Failed to deserialize response from GHN API.");
                }
                if (response.Code != 200)
                {
                    throw new Exception($"GHN API returned error code {response.Code}: {response.Message}");
                }
                return response.Data;
            }
            else
            {
                var errorContent = await responseMessage.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[GHN Error]: {errorContent}");
                throw new HttpRequestException($"GHN API request failed with status code {responseMessage.StatusCode}. Response: {errorContent}");
            }
        }

        public async Task<List<GhnWardDto>> GetWardsAsync(int districtId)
        {
            // Theo tài liệu cURL: Lấy Xã yêu cầu POST dữ liệu json chứa "district_id" hoặc query string
            var requestPayload = new { district_id = districtId };
            var responseMessage = await _httpClient.PostAsJsonAsync("ward?district_id=" + districtId, requestPayload);

            if (responseMessage.IsSuccessStatusCode)
            {
                var response = await responseMessage.Content.ReadFromJsonAsync<GhnApiResponse<List<GhnWardDto>>>();
                if (response == null)
                {
                    throw new Exception("Failed to deserialize response from GHN API.");
                }
                if (response.Code != 200)
                {
                    throw new Exception($"GHN API returned error code {response.Code}: {response.Message}");
                }
                return response.Data;
            }
            else
            {
                var errorContent = await responseMessage.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[GHN Error]: {errorContent}");
                throw new HttpRequestException($"GHN API request failed with status code {responseMessage.StatusCode}. Response: {errorContent}");
            }
        }
    }
}