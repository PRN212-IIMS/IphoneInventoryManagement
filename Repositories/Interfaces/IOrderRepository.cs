using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IOrderRepository
    {
        List<Order> GetAllOrders();
        List<Order> GetOrdersByCustomerId(int customerId);
        Order? GetOrderById(int id);
        List<Order> SearchOrders(string keyword);
        List<Order> FilterOrders(string? status, DateTime? fromDate, DateTime? toDate);
        void AddOrder(Order order);
        void AddOrderDetails(List<OrderDetail> orderDetails);
        void UpdateOrder(Order order);
        void UpdateOrderStatus(int orderId, string status);
    }
}
