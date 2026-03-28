using BusinessObjects;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Repositories.Interfaces;

namespace UnitTest
{
    public class FakeOrderRepository : IOrderRepository
    {
        public List<Order> Orders { get; set; } = new();
        public List<OrderDetail> OrderDetails { get; set; } = new();

        public List<Order> GetAllOrders()
        {
            return Orders.ToList();
        }

        public List<Order> GetOrdersByCustomerId(int customerId)
        {
            return Orders.Where(o => o.CustomerId == customerId).ToList();
        }

        public Order? GetOrderById(int id)
        {
            var order = Orders.FirstOrDefault(o => o.OrderId == id);
            if (order != null)
            {
                order.OrderDetails = OrderDetails.Where(d => d.OrderId == id).ToList();
            }
            return order;
        }

        public List<Order> SearchOrders(string keyword)
        {
            keyword ??= string.Empty;
            return Orders.Where(o =>
                (o.ReceiverName ?? string.Empty).Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                (o.Status ?? string.Empty).Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<Order> FilterOrders(string? status, DateTime? fromDate, DateTime? toDate)
        {
            IEnumerable<Order> query = Orders;

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(o => string.Equals(o.Status, status, StringComparison.OrdinalIgnoreCase));

            if (fromDate.HasValue)
                query = query.Where(o => o.OrderDate.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(o => o.OrderDate.Date <= toDate.Value.Date);

            return query.ToList();
        }

        public void AddOrder(Order order)
        {
            if (order.OrderId == 0)
                order.OrderId = Orders.Count == 0 ? 1 : Orders.Max(o => o.OrderId) + 1;

            Orders.Add(order);
        }

        public void AddOrderDetails(List<OrderDetail> orderDetails)
        {
            OrderDetails.AddRange(orderDetails);
        }

        public void UpdateOrder(Order order)
        {
            var existing = Orders.FirstOrDefault(o => o.OrderId == order.OrderId);
            if (existing == null) return;

            existing.Status = order.Status;
            existing.TotalAmount = order.TotalAmount;
            existing.ReceiverName = order.ReceiverName;
            existing.ReceiverPhone = order.ReceiverPhone;
            existing.ShippingAddress = order.ShippingAddress;
            existing.CustomerId = order.CustomerId;
            existing.StaffId = order.StaffId;
            existing.ProcessedAt = order.ProcessedAt;
            existing.PaidAt = order.PaidAt;
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            var order = Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
                order.Status = status;
        }
    }
}