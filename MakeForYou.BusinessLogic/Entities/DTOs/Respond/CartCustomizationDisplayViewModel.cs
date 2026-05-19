namespace MakeForYou.BusinessLogic.Entities.DTOs.Respond
{
    public class CartCustomizationDisplayViewModel
    {
        public long GroupId { get; set; }
        public long OptionId { get; set; }
        public string OptionName { get; set; } = null!;
    }
}