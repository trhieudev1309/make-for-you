using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MakeForYou.BusinessLogic.Entities.DTOs.Request
{
    public class CustomizationGroupRequest
    {
        [Required(ErrorMessage = "Tiêu đề tùy chỉnh không được để trống")]
        [MaxLength(100, ErrorMessage = "Tiêu đề không được vượt quá 100 ký tự")]
        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; }

        // List of options for this group
        [JsonPropertyName("options")]
        public List<CustomizationOptionRequest> Options { get; set; } = new();
    }
}