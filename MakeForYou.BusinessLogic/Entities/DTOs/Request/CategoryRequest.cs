namespace MakeForYou.BusinessLogic.Entities.DTOs.Request
{
    public class CategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        // Có thể thêm Description hoặc IsActive nếu muốn quản lý sâu hơn
    }
}