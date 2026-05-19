using System.Text.Json.Serialization;

namespace MakeForYou.BusinessLogic.Entities.DTOs
{
    public class CartCustomizationDto
    {
        public long CustomizationGroupId { get; set; }
        public long CustomizationOptionId { get; set; }
        [JsonPropertyName("groupTitle")]
        public string GroupTitle { get; set; } = null!;
        [JsonPropertyName("optionName")]
        public string OptionName { get; set; } = null!;
    }
}
