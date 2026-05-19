namespace MakeForYou.BusinessLogic.Entities.DTOs.Respond
{
    public class CustomizationOptionViewModel
    {
        public long CustomizationOptionId { get; set; }
        public string OptionValue { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public int Status { get; set; }
    }
}