namespace MakeForYou.BusinessLogic.Entities.DTOs.Request
{
    public class ProductRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Price { get; set; }
        public string? ImageUrl { get; set; }
        public long CategoryId { get; set; }

        public long SellerId { get; set; }
        public int Status { get; set; } // 1: Active, 0: Hidden
    }
}