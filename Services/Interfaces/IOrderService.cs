using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IOrderService
    {
        List<Order> GetAllOrders();
        List<Order> GetOrdersByCustomerId(int customerId);
        Order? GetOrderById(int id);
        List<Order> SearchOrders(string keyword);
        List<Order> FilterOrders(string? status, DateTime? fromDate, DateTime? toDate);
        void CreateOrder(Order order, List<OrderDetail> orderDetails);
        void UpdateOrderStatus(int orderId, string status);
        void CancelOrder(int orderId, int customerId);
    }
}
