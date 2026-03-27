using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer
{
    public class OrderDAO
    {
        public List<Order> GetAllOrders()
        {
            using var context = new IPhoneInventoryDbContext();
            return context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public List<Order> GetOrdersByCustomerId(int customerId)
        {
            using var context = new IPhoneInventoryDbContext();
            return context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.OrderDetails)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public Order? GetOrderById(int id)
        {
            using var context = new IPhoneInventoryDbContext();
            return context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderId == id);
        }

        public List<Order> SearchOrders(string keyword)
        {
            using var context = new IPhoneInventoryDbContext();
            var query = context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                query = query.Where(o =>
                    o.OrderId.ToString().Contains(keyword) ||
                    (o.Status != null && o.Status.Contains(keyword)) ||
                    (o.ReceiverName != null && o.ReceiverName.Contains(keyword)) ||
                    (o.ReceiverPhone != null && o.ReceiverPhone.Contains(keyword)) ||
                    (o.ShippingAddress != null && o.ShippingAddress.Contains(keyword)) ||
                    (o.Customer != null && o.Customer.FullName.Contains(keyword)) ||
                    (o.Customer != null && o.Customer.Email.Contains(keyword)) ||
                    (o.Staff != null && o.Staff.FullName.Contains(keyword)));
            }

            return query
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public List<Order> FilterOrders(string? status, DateTime? fromDate, DateTime? toDate)
        {
            using var context = new IPhoneInventoryDbContext();
            var query = context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.OrderDetails)
                .AsQueryable();

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
            using var context = new IPhoneInventoryDbContext();
            context.Orders.Add(order);
            context.SaveChanges();
        }

        public void AddOrderDetails(List<OrderDetail> orderDetails)
        {
            using var context = new IPhoneInventoryDbContext();
            context.OrderDetails.AddRange(orderDetails);
            context.SaveChanges();
        }

        public void UpdateOrder(Order order)
        {
            using var context = new IPhoneInventoryDbContext();
            context.Orders.Update(order);
            context.SaveChanges();
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            using var context = new IPhoneInventoryDbContext();

            var order = context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
            {
                throw new Exception("Không tìm thấy đơn hàng.");
            }

            order.Status = status;
            order.ProcessedAt = DateTime.Now;
            context.SaveChanges();
        }
    }
}