namespace MakeForYou.BusinessLogic.Entities.DTOs.Respond
{
    public class CategoryViewModel
    {
        public long CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ProductCount { get; set; } // Số lượng sản phẩm thuộc danh mục này
    }
}