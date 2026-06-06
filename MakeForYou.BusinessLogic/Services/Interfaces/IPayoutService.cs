namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface IPayoutService
    {
        Task<(bool Success, string Message)> PaySellerAsync(long orderId);
    }
}
