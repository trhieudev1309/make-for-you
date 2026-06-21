using System.Text.Json.Serialization;

namespace MakeForYou.BusinessLogic.DTOs
{
    // Lớp bọc tổng quát: Khớp chính xác 100% chữ thường của hệ thống GHN
    public class GhnApiResponse<T>
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;

        [JsonPropertyName("data")]
        public T Data { get; set; } = default!;
    }

    // DTO Tỉnh / Thành: Khớp chính xác thuộc tính gốc của GHN
    public class GhnProvinceDto
    {
        [JsonPropertyName("ProvinceID")]
        public int ProvinceId { get; set; }

        [JsonPropertyName("ProvinceName")]
        public string ProvinceName { get; set; } = null!;
    }

    // DTO Quận / Huyện
    public class GhnDistrictDto
    {
        [JsonPropertyName("DistrictID")]
        public int DistrictId { get; set; }

        [JsonPropertyName("ProvinceID")]
        public int ProvinceId { get; set; }

        [JsonPropertyName("DistrictName")]
        public string DistrictName { get; set; } = null!;
    }

    // DTO Phường / Xã
    public class GhnWardDto
    {
        [JsonPropertyName("WardCode")]
        public string WardCode { get; set; } = null!;

        [JsonPropertyName("DistrictID")]
        public int DistrictId { get; set; }

        [JsonPropertyName("WardName")]
        public string WardName { get; set; } = null!;
    }
}