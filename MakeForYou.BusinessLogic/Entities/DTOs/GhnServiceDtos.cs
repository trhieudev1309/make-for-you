using System.Text.Json.Serialization;

namespace MakeForYou.BusinessLogic.DTOs
{
    public class GhnServiceDto
    {
        [JsonPropertyName("service_id")]
        public int ServiceId { get; set; }

        [JsonPropertyName("service_type_id")]
        public int ServiceTypeId { get; set; }

        [JsonPropertyName("short_name")]
        public string ShortName { get; set; } = string.Empty;
    }

    public class GhnFeeResponseDto
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("service_fee")]
        public int ServiceFee { get; set; }
    }
    
    public class GhnOrderCreateResponseDto
    {
        [JsonPropertyName("order_code")]
        public string OrderCode { get; set; } = null!;
    }

    public class GhnLogDto
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        [JsonPropertyName("updated_date")]
        public DateTime? UpdatedDate { get; set; }
    }

    public class GhnOrderDetailResponseDto
    {
        [JsonPropertyName("log")]
        public List<GhnLogDto>? Log { get; set; }
    }
}
