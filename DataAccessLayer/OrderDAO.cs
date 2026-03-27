using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    public class OrderDAO
    {
        private readonly IPhoneInventoryDbContext _context;

        public OrderDAO()
        {
            _context = new IPhoneInventoryDbContext();
        }

        public List<Order> GetAllOrders()
        {
            return _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public List<Order> GetOrdersByCustomerId(int customerId)
        {
            return _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public Order? GetOrderById(int id)
        {
            return _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderId == id);
        }

        public List<Order> SearchOrders(string keyword)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(o =>
                    o.OrderId.ToString().Contains(keyword) ||
                    o.Status.Contains(keyword) ||
                    (o.ReceiverName != null && o.ReceiverName.Contains(keyword)) ||
                    (o.ReceiverPhone != null && o.ReceiverPhone.Contains(keyword)) ||
                    (o.ShippingAddress != null && o.ShippingAddress.Contains(keyword)) ||
                    o.Customer.FullName.Contains(keyword) ||
                    o.Customer.Email.Contains(keyword));
            }

            return query
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public List<Order> FilterOrders(string? status, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                DateTime endDate = toDate.Value.Date.AddDays(1);
                query = query.Where(o => o.OrderDate < endDate);
            }

            return query
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public void AddOrder(Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
        }

        public void AddOrderDetails(List<OrderDetail> orderDetails)
        {
            _context.OrderDetails.AddRange(orderDetails);
            _context.SaveChanges();
        }

        public void UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            _context.SaveChanges();
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
            {
                throw new Exception("Không tìm thấy đơn hàng.");
            }

            order.Status = status;
            _context.SaveChanges();
        }
    }
}