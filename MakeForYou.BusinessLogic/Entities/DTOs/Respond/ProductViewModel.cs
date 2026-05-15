namespace MakeForYou.BusinessLogic.Entities.DTOs.Respond
{
    public class ProductViewModel
    {
        public long ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Price { get; set; }
        public string? ImageUrl { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public long CategoryId { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}