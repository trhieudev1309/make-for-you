using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.DTOs;

namespace MakeForYou.BusinessLogic.Services
{
    public class GhnService : IGhnService
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;
        private readonly int _shopId;
        private readonly string _shopIdStr;

        public GhnService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _token = configuration["GHN:Token"] ?? throw new ArgumentNullException(nameof(configuration), "GHN Token is missing in configuration");
            _shopIdStr = configuration["GHN:ShopId"] ?? "828";
            _shopId = int.Parse(_shopIdStr);

            var baseUrl = configuration["GHN:BaseUrl"] ?? "https://dev-online-gateway.ghn.vn/shiip/public-api/";
            // Chuẩn hóa BaseUrl: nếu chứa master-data/ thì cắt đi để gọi được các endpoint shipping-order
            if (baseUrl.EndsWith("master-data/"))
            {
                baseUrl = baseUrl.Substring(0, baseUrl.Length - "master-data/".Length);
            }

            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            
            // Xóa header cũ tránh lỗi trùng lặp khi tái sử dụng HttpClient
            _httpClient.DefaultRequestHeaders.Remove("Token");
            _httpClient.DefaultRequestHeaders.Remove("ShopId");
            _httpClient.DefaultRequestHeaders.Remove("ShopID");

            _httpClient.DefaultRequestHeaders.Add("Token", _token);
            _httpClient.DefaultRequestHeaders.Add("ShopId", _shopIdStr);
            _httpClient.DefaultRequestHeaders.Add("ShopID", _shopIdStr);
        }

        public async Task<List<GhnServiceDto>> GetAvailableServicesAsync(int fromDistrictId, int toDistrictId)
        {
            var payload = new
            {
                shop_id = _shopId,
                from_district = fromDistrictId,
                to_district = toDistrictId
            };

            var responseMessage = await _httpClient.PostAsJsonAsync("v2/shipping-order/available-services", payload);
            if (responseMessage.IsSuccessStatusCode)
            {
                var response = await responseMessage.Content.ReadFromJsonAsync<GhnApiResponse<List<GhnServiceDto>>>();
                if (response != null && response.Code == 200)
                {
                    return response.Data;
                }
                throw new Exception($"GHN Available Services API error: {response?.Message}");
            }

            var error = await responseMessage.Content.ReadAsStringAsync();
            throw new Exception($"GHN Available Services API failed: {responseMessage.StatusCode} - {error}");
        }

        public async Task<string> GetOrderDetailAsync(string orderCode)
        {
            var payload = new { order_code = orderCode };
            var responseMessage = await _httpClient.PostAsJsonAsync("v2/shipping-order/soc", payload);
            if (responseMessage.IsSuccessStatusCode)
            {
                return await responseMessage.Content.ReadAsStringAsync();
            }

            var error = await responseMessage.Content.ReadAsStringAsync();
            throw new Exception($"GHN SOC API failed: {responseMessage.StatusCode} - {error}");
        }

        public async Task<int> CalculateShippingFeeAsync(List<Product> cartItems, int toDistrictId, string toWardCode)
        {
            if (cartItems == null || !cartItems.Any())
            {
                return 0;
            }

            // 1. Tự động xoay lật kích thước: Cạnh dài nhất = Length, cạnh ngắn nhất = Height
            var sortedItems = new List<dynamic>();
            int totalWeight = 0;

            foreach (var item in cartItems)
            {
                var dims = new[] { item.Length, item.Width, item.Height };
                Array.Sort(dims); // Sắp xếp tăng dần

                int itemHeight = dims[0]; // Cạnh ngắn nhất
                int itemWidth = dims[1];  // Cạnh trung bình
                int itemLength = dims[2]; // Cạnh dài nhất

                sortedItems.Add(new
                {
                    Weight = item.Weight > 0 ? item.Weight : 200, // gram
                    Length = itemLength > 0 ? itemLength : 10,     // cm
                    Width = itemWidth > 0 ? itemWidth : 10,        // cm
                    Height = itemHeight > 0 ? itemHeight : 5,      // cm
                    Name = item.Title ?? "Product"
                });

                totalWeight += item.Weight > 0 ? item.Weight : 200;
            }

            // 2. Thuật toán gộp giỏ hàng của GHN:
            // Kích thước đơn hàng = Max(length), Max(width), Sum(height)
            int totalLength = 0;
            int totalWidth = 0;
            int totalHeight = 0;

            foreach (var item in sortedItems)
            {
                if (item.Length > totalLength) totalLength = item.Length;
                if (item.Width > totalWidth) totalWidth = item.Width;
                totalHeight += item.Height;
            }

            // Khối lượng quy đổi thể tích = (Length x Width x Height) / 5
            int volumeWeight = (totalLength * totalWidth * totalHeight) / 5;

            // Trọng lượng tính cước = Max(Trọng lượng thực tế, Trọng lượng thể tích)
            int chargeableWeight = Math.Max(totalWeight, volumeWeight);

            // 3. Phân loại gói cước: Nếu trọng lượng tính cước > 20kg (20000g), dùng hàng nặng (id = 5), ngược lại dùng hàng nhẹ (id = 2)
            int targetServiceTypeId = chargeableWeight > 20000 ? 5 : 2;

            // 4. Lấy gói dịch vụ khả dụng từ GHN
            int fromDistrictId = 1447; // Quận mặc định của shop (Ví dụ: 1447 là Quận 9, TP.HCM hoặc lấy từ cấu hình)
            var availableServices = await GetAvailableServicesAsync(fromDistrictId, toDistrictId);
            if (availableServices == null || !availableServices.Any())
            {
                throw new Exception("Không tìm thấy gói dịch vụ giao hàng khả dụng của GHN.");
            }

            // Lấy service phù hợp, nếu không khớp thì fallback lấy service đầu tiên
            var selectedService = availableServices.FirstOrDefault(s => s.ServiceTypeId == targetServiceTypeId) 
                                 ?? availableServices.First();

            // 5. Chuẩn bị Request Body tính phí ship (không gửi từ_district_id/from_ward_code để GHN tự động lấy thông tin từ ShopId)
            object feePayload;
            if (selectedService.ServiceTypeId == 5)
            {
                // Gói Hàng nặng (service_type_id = 5): Thông tin kích thước/khối lượng lấy chi tiết từ mảng items
                feePayload = new
                {
                    service_id = selectedService.ServiceId,
                    service_type_id = selectedService.ServiceTypeId,
                    to_district_id = toDistrictId,
                    to_ward_code = toWardCode,
                    weight = chargeableWeight,
                    length = totalLength,
                    width = totalWidth,
                    height = totalHeight,
                    items = sortedItems.Select(item => new
                    {
                        name = item.Name,
                        quantity = 1,
                        weight = item.Weight,
                        length = item.Length,
                        width = item.Width,
                        height = item.Height
                    }).ToList()
                };
            }
            else
            {
                // Gói Hàng nhẹ (service_type_id = 2): Tính cước dựa trên thông số tổng ở ngoài body
                feePayload = new
                {
                    service_id = selectedService.ServiceId,
                    service_type_id = selectedService.ServiceTypeId,
                    to_district_id = toDistrictId,
                    to_ward_code = toWardCode,
                    weight = chargeableWeight,
                    length = totalLength,
                    width = totalWidth,
                    height = totalHeight
                };
            }

            var responseMessage = await _httpClient.PostAsJsonAsync("v2/shipping-order/fee", feePayload);
            if (responseMessage.IsSuccessStatusCode)
            {
                var response = await responseMessage.Content.ReadFromJsonAsync<GhnApiResponse<GhnFeeResponseDto>>();
                if (response != null && response.Code == 200)
                {
                    return response.Data.Total;
                }
            }

            // Fallback: Nếu gọi lần đầu lỗi (thường do ShopId trên Sandbox chưa được cấu hình địa chỉ mặc định),
            // chúng ta sẽ tự động gửi thêm địa chỉ gửi mặc định (Quận 9, TP.HCM) để đảm bảo không bị lỗi 400.
            object fallbackPayload;
            string fromWardCode = "90509"; // Phường Hiệp Phú

            if (selectedService.ServiceTypeId == 5)
            {
                fallbackPayload = new
                {
                    service_id = selectedService.ServiceId,
                    service_type_id = selectedService.ServiceTypeId,
                    from_district_id = fromDistrictId,
                    from_ward_code = fromWardCode,
                    to_district_id = toDistrictId,
                    to_ward_code = toWardCode,
                    weight = chargeableWeight,
                    length = totalLength,
                    width = totalWidth,
                    height = totalHeight,
                    items = sortedItems.Select(item => new
                    {
                        name = item.Name,
                        quantity = 1,
                        weight = item.Weight,
                        length = item.Length,
                        width = item.Width,
                        height = item.Height
                    }).ToList()
                };
            }
            else
            {
                fallbackPayload = new
                {
                    service_id = selectedService.ServiceId,
                    service_type_id = selectedService.ServiceTypeId,
                    from_district_id = fromDistrictId,
                    from_ward_code = fromWardCode,
                    to_district_id = toDistrictId,
                    to_ward_code = toWardCode,
                    weight = chargeableWeight,
                    length = totalLength,
                    width = totalWidth,
                    height = totalHeight
                };
            }

            var fallbackResponse = await _httpClient.PostAsJsonAsync("v2/shipping-order/fee", fallbackPayload);
            if (fallbackResponse.IsSuccessStatusCode)
            {
                var response = await fallbackResponse.Content.ReadFromJsonAsync<GhnApiResponse<GhnFeeResponseDto>>();
                if (response != null && response.Code == 200)
                {
                    return response.Data.Total;
                }
                throw new Exception($"GHN Fee API error (fallback): {response?.Message}");
            }

            var errorStr = await responseMessage.Content.ReadAsStringAsync();
            throw new Exception($"GHN Fee API failed: {responseMessage.StatusCode} - {errorStr}");
        }

        public async Task<string?> CreateShippingOrderAsync(Order order)
        {
            if (order == null) return null;

            // 1. Tính toán gộp kích thước giỏ hàng giống như lúc tính phí
            int totalLength = 0;
            int totalWidth = 0;
            int totalHeight = 0;
            int totalWeight = 0;

            if (order.OrderItems != null && order.OrderItems.Any())
            {
                foreach (var item in order.OrderItems)
                {
                    var product = item.Product;
                    if (product != null)
                    {
                        var dims = new[] { product.Length, product.Width, product.Height };
                        Array.Sort(dims); // Cạnh ngắn nhất -> Cạnh dài nhất

                        int itemHeight = dims[0];
                        int itemWidth = dims[1];
                        int itemLength = dims[2];

                        int qty = item.Quantity > 0 ? item.Quantity : 1;

                        if (itemLength > totalLength) totalLength = itemLength;
                        if (itemWidth > totalWidth) totalWidth = itemWidth;
                        totalHeight += (itemHeight > 0 ? itemHeight : 5) * qty;
                        totalWeight += (product.Weight > 0 ? product.Weight : 200) * qty;
                    }
                }
            }

            if (totalLength == 0) totalLength = 10;
            if (totalWidth == 0) totalWidth = 10;
            if (totalHeight == 0) totalHeight = 5;
            if (totalWeight == 0) totalWeight = 200;

            // 2. Lấy gói dịch vụ khả dụng từ GHN giống như lúc tính phí
            int fromDistrictId = 1447; // Quận 9, HCM (Default shop district)
            string fromWardCode = "90509"; // Phường Hiệp Phú (Default shop ward)
            int toDistrictId = order.ShippingDistrictId ?? 1447;
            string toWardCode = order.ShippingWardCode ?? "90509";

            int chargeableWeight = Math.Max(totalWeight, (totalLength * totalWidth * totalHeight) / 5);
            int serviceTypeId = chargeableWeight > 20000 ? 5 : 2;

            int serviceId = 0;
            try
            {
                var availableServices = await GetAvailableServicesAsync(fromDistrictId, toDistrictId);
                if (availableServices != null && availableServices.Any())
                {
                    var selectedService = availableServices.FirstOrDefault(s => s.ServiceTypeId == serviceTypeId) 
                                         ?? availableServices.First();
                    serviceId = selectedService.ServiceId;
                    serviceTypeId = selectedService.ServiceTypeId;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get service list from GHN: {ex.Message}");
            }

            // 3. Chuẩn bị danh sách items cho GHN
            var itemsList = new List<object>();
            if (order.OrderItems != null && order.OrderItems.Any())
            {
                foreach (var item in order.OrderItems)
                {
                    itemsList.Add(new
                    {
                        name = item.Product?.Title ?? "Sản phẩm MakeForYou",
                        code = item.ProductId.ToString(),
                        quantity = item.Quantity > 0 ? item.Quantity : 1,
                        price = item.Price
                    });
                }
            }
            else
            {
                // Đối với đơn hàng Customization không có OrderItems trong DB trực tiếp (hoặc dạng khác)
                itemsList.Add(new
                {
                    name = order.OrderDescription ?? "Sản phẩm thiết kế MakeForYou",
                    code = $"CUSTOM-{order.OrderId}",
                    quantity = 1,
                    price = order.AgreedPrice ?? 0
                });
            }

            // 4. Nếu đơn hàng đã được thanh toán online trước đó (IsPaid = true), COD = 0
            // Ngược lại nếu chưa thanh toán, COD = Tổng số tiền = AgreedPrice + ShippingFee
            int codAmount = order.IsPaid ? 0 : ((order.AgreedPrice ?? 0) + (order.ShippingFee ?? 0));

            // Đảm bảo Token và ShopId trong request headers
            _httpClient.DefaultRequestHeaders.Remove("Token");
            _httpClient.DefaultRequestHeaders.Remove("ShopId");
            _httpClient.DefaultRequestHeaders.Add("Token", _token);
            _httpClient.DefaultRequestHeaders.Add("ShopId", _shopIdStr);

            var payload = new
            {
                payment_type_id = 1, // 1: Cửa hàng/Người bán thanh toán cước phí cho GHN (Vì shop đã thu tiền ship từ khách lúc checkout)
                note = "Đơn hàng từ MakeForYou. Vui lòng gọi trước khi giao.",
                required_note = "KHONGCHOXEMHANG",
                
                // Người gửi (From Info) - điền thông tin mặc định để bypass lỗi địa chỉ ShopId trên Sandbox
                from_name = "Shop MakeForYou",
                from_phone = "0973660589",
                from_address = "70 Phố Hiệp Phú",
                from_district_id = fromDistrictId,
                from_ward_code = fromWardCode,

                // Người nhận (To Info)
                to_name = order.ShippingFullName ?? order.Buyer?.FullName ?? "Khách hàng",
                to_phone = order.ShippingPhone ?? order.Buyer?.Phone ?? "0909090909",
                to_address = order.ShippingAddressDetail ?? "Địa chỉ nhận hàng",
                to_ward_code = toWardCode,
                to_district_id = toDistrictId,

                cod_amount = codAmount,
                content = order.OrderDescription ?? "Đơn hàng MakeForYou",
                weight = totalWeight,
                length = totalLength,
                width = totalWidth,
                height = totalHeight,
                insurance_value = order.AgreedPrice ?? 0,
                service_id = serviceId,
                service_type_id = serviceTypeId,
                items = itemsList
            };

            var responseMessage = await _httpClient.PostAsJsonAsync("v2/shipping-order/create", payload);
            if (responseMessage.IsSuccessStatusCode)
            {
                var response = await responseMessage.Content.ReadFromJsonAsync<GhnApiResponse<GhnOrderCreateResponseDto>>();
                if (response != null && response.Code == 200 && response.Data != null)
                {
                    return response.Data.OrderCode;
                }
                else
                {
                    var errorMsg = response?.Message ?? "Unknown error";
                    System.Diagnostics.Debug.WriteLine($"GHN Shipping Order creation failed internally: {errorMsg}");
                }
            }
            else
            {
                var errorStr = await responseMessage.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"GHN Shipping Order API error response: {responseMessage.StatusCode} - {errorStr}");
            }

            return null;
        }

        public async Task<List<GhnLogDto>> GetOrderTrackingLogsAsync(string ghnShipmentCode)
        {
            if (string.IsNullOrWhiteSpace(ghnShipmentCode))
            {
                return new List<GhnLogDto>();
            }

            // Đảm bảo Token và ShopId trong request headers
            _httpClient.DefaultRequestHeaders.Remove("Token");
            _httpClient.DefaultRequestHeaders.Remove("ShopId");
            _httpClient.DefaultRequestHeaders.Add("Token", _token);
            _httpClient.DefaultRequestHeaders.Add("ShopId", _shopIdStr);

            var payload = new
            {
                order_code = ghnShipmentCode
            };

            try
            {
                var responseMessage = await _httpClient.PostAsJsonAsync("v2/shipping-order/detail", payload);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var response = await responseMessage.Content.ReadFromJsonAsync<GhnApiResponse<GhnOrderDetailResponseDto>>();
                    if (response != null && response.Code == 200 && response.Data != null && response.Data.Log != null)
                    {
                        return response.Data.Log;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting GHN tracking logs: {ex.Message}");
            }

            return new List<GhnLogDto>();
        }

        public async Task<bool> CancelShipmentAsync(string ghnOrderCode)
        {
            if (string.IsNullOrWhiteSpace(ghnOrderCode))
            {
                return false;
            }

            var payload = new
            {
                order_codes = new[] { ghnOrderCode }
            };

            var responseMessage = await _httpClient.PostAsJsonAsync("v2/switch-status/cancel", payload);
            if (responseMessage.IsSuccessStatusCode)
            {
                var response = await responseMessage.Content.ReadFromJsonAsync<GhnApiResponse<object>>();
                if (response != null && response.Code == 200)
                {
                    return true;
                }
            }

            return false;
        }
    }
}