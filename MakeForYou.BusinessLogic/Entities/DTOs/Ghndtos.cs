using System.Text.Json.Serialization;

namespace MakeForYou.BusinessLogic.Entities.DTOs.Ghn
{
    // ─── Request DTOs ──────────────────────────────────────────────────────────

    /// <summary>
    /// Payload gửi lên GHN API để tạo store mới.
    /// Docs: POST /shiip/public-api/v2/shop/register
    /// </summary>
    public class GhnCreateStoreRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = null!;

        [JsonPropertyName("address")]
        public string Address { get; set; } = null!;

        /// <summary>
        /// Ward code lấy từ GHN Address API (GetWard).
        /// </summary>
        [JsonPropertyName("ward_code")]
        public string WardCode { get; set; } = null!;

        /// <summary>
        /// District ID lấy từ GHN Address API (GetDistrict).
        /// </summary>
        [JsonPropertyName("district_id")]
        public int DistrictId { get; set; }
    }

    // ─── Response DTOs ─────────────────────────────────────────────────────────

    /// <summary>
    /// Wrapper chung cho mọi response từ GHN API.
    /// </summary>
    public class GhnApiResponse<T>
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }

    /// <summary>
    /// Data trả về khi tạo store thành công.
    /// </summary>
    public class GhnCreateStoreResponse
    {
        [JsonPropertyName("shop_id")]
        public int ShopId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("ward_code")]
        public string? WardCode { get; set; }

        [JsonPropertyName("district_id")]
        public int DistrictId { get; set; }

        [JsonPropertyName("client_id")]
        public int ClientId { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }
    }

    /// <summary>
    /// Thông tin một store khi query danh sách stores.
    /// </summary>
    public class GhnStoreInfo
    {
        [JsonPropertyName("_id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("ward_code")]
        public string? WardCode { get; set; }

        [JsonPropertyName("district_id")]
        public int DistrictId { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }
    }

    /// <summary>
    /// Data trả về khi query danh sách stores của account.
    /// </summary>
    public class GhnGetStoreResponse
    {
        [JsonPropertyName("shops")]
        public List<GhnStoreInfo>? Shops { get; set; }
    }
}