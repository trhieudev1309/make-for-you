namespace MakeForYou.BusinessLogic.Entities.DTOs.Request
{
    public class AddToCartRequest
    {
        public long ProductId { get; set; }

        public int Quantity { get; set; }

        public string? CustomizationJson { get; set; }
    }
}
