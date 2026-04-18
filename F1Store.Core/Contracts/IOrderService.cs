using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using F1Store.Infrastructure.Data.Domain;

namespace F1Store.Core.Contracts
{
    public interface IOrderService
    {
        bool Create(int productId, string userId, int quantity);

        List<Order> GetOrders();

        List<Order> GetOrdersByUser(string userId);
        Order GetOrderById(int orderId);

        bool Delete(int orderId);

        bool CreateFromCart(string userId);

        Guid? GetLatestOrderGroupIdByUser(string userId);

        List<Order> GetOrdersByGroupId(Guid orderGroupId, string userId);

        bool UserHasOrders(string userId);

        (bool Success, Guid? OrderGroupId, List<(int ProductId, string ProductName, int Requested, int Available, string Action)> Issues)
        TryCheckoutFromCart(string userId);

        Order GetOrderDetails(int id);

        bool FinalizePayment(Guid orderGroupId, string userId);
    }
}