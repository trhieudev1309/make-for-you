namespace MakeForYou.BusinessLogic.Entities.DTOs.Request
{
    public class CartItemCustomization
    {
        public long CartItemId { get; set; }
        public bool HasCustomization { get; set; }
        public string? Note { get; set; }
    }
}
