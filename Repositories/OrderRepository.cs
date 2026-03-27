using BusinessObjects;
using DataAccessLayer;
using Repositories.Interfaces;

namespace Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDAO _orderDAO;

        public OrderRepository()
        {
            _orderDAO = new OrderDAO();
        }

        public List<Order> GetAllOrders()
        {
            return _orderDAO.GetAllOrders();
        }

        public List<Order> GetOrdersByCustomerId(int customerId)
        {
            return _orderDAO.GetOrdersByCustomerId(customerId);
        }

        public Order? GetOrderById(int id)
        {
            return _orderDAO.GetOrderById(id);
        }

        public List<Order> SearchOrders(string keyword)
        {
            return _orderDAO.SearchOrders(keyword);
        }

        public List<Order> FilterOrders(string? status, DateTime? fromDate, DateTime? toDate)
        {
            return _orderDAO.FilterOrders(status, fromDate, toDate);
        }

        public void AddOrder(Order order)
        {
            _orderDAO.AddOrder(order);
        }

        public void AddOrderDetails(List<OrderDetail> orderDetails)
        {
            _orderDAO.AddOrderDetails(orderDetails);
        }

        public void UpdateOrder(Order order)
        {
            _orderDAO.UpdateOrder(order);
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            _orderDAO.UpdateOrderStatus(orderId, status);
        }
    }
}