using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrdersByUserAsync(long buyerId);
        Task<Order?> GetOrderDetailAsync(long orderId, long buyerId);
        Task<Order> CreateOrderAsync(long buyerId, long sellerId, string description);
        Task<List<Order>> CreateOrderFromCartAsync(long userId, string fullName, string phone, string address, long paymentCode, List<CartItemCustomization>? customizations = null);
        Task<List<Order>> GetRequestsBySellerAsync(long sellerId);
        Task<Order?> GetRequestDetailAsync(long orderId, long sellerId);
        Task UpdateStatusAsync(long orderId, int status);
        Task<(List<Order> Orders, int TotalCount)> GetAllOrdersForAdminAsync(
        string? search, int? status, int pageIndex, int pageSize);
        Task<AuthResult> UpdateProgressAsync(long orderId, long sellerId, UpdateProgressRequest req);
        Task<Order?> GetOrderForSellerAsync(long orderId, long sellerId);
        Task<long?> GetOrderIdByUsersAsync(long userA, long userB);
        Task DropCustomizationAsync(long orderItemId);
    }
}
