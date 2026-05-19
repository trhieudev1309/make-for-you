namespace MakeForYou.BusinessLogic.Entities.DTOs.Respond
{
    public class CustomizationGroupViewModel
    {
        public long CustomizationGroupId { get; set; }
        public string Title { get; set; } = null!;
        public int DisplayOrder { get; set; }
        public int Status { get; set; }
        public List<CustomizationOptionViewModel> Options { get; set; } = new();
    }
}