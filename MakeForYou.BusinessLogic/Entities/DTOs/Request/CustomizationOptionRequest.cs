using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MakeForYou.BusinessLogic.Entities.DTOs.Request
{
    public class CustomizationOptionRequest
    {
        [Required(ErrorMessage = "Giá trị tùy chỉnh không được để trống")]
        [MaxLength(100, ErrorMessage = "Giá trị không được vượt quá 100 ký tự")]
        [JsonPropertyName("optionValue")]
        public string OptionValue { get; set; } = null!;

        [JsonPropertyName("displayOrder")]
        public int DisplayOrder { get; set; }
    }
}