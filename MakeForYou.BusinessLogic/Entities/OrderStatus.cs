namespace MakeForYou.BusinessLogic.Entities
{
    public enum OrderStatus
    {
        Pending = 1,   // Chờ thanh toán/xác nhận
        Paid = 2,      // Đã thanh toán
        Processing = 3, // Đang thực hiện
        Shipped = 4,   // Đang giao hàng
        Cancelled = 0  // Đã hủy
    }
}