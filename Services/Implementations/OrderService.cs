using BusinessObjects;
using Repositories.Interfaces;
using Services.Interfaces;
using Repositories.Implementations;

namespace Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderService()
        {
            _orderRepository = new OrderRepository();
            _productRepository = new ProductRepository();
        }

        public List<Order> GetAllOrders()
        {
            return _orderRepository.GetAllOrders();
        }

        public List<Order> GetOrdersByCustomerId(int customerId)
        {
            if (customerId <= 0)
                return new List<Order>();

            return _orderRepository.GetOrdersByCustomerId(customerId);
        }

        public Order? GetOrderById(int id)
        {
            if (id <= 0)
                return null;

            return _orderRepository.GetOrderById(id);
        }

        public List<Order> SearchOrders(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return _orderRepository.GetAllOrders();

            return _orderRepository.SearchOrders(keyword.Trim());
        }

        public List<Order> FilterOrders(string? status, DateTime? fromDate, DateTime? toDate)
        {
            return _orderRepository.FilterOrders(status, fromDate, toDate);
        }

        public void CreateOrder(Order order, List<OrderDetail> orderDetails)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            bool hasStaff = order.StaffId != null && order.StaffId > 0;
            bool hasCustomer = order.CustomerId != null && order.CustomerId > 0;

            if (!hasStaff && !hasCustomer)
                throw new Exception("Người tạo đơn không hợp lệ.");

            if (string.IsNullOrWhiteSpace(order.ReceiverName))
                throw new Exception("Tên người nhận không được để trống.");

            if (string.IsNullOrWhiteSpace(order.ReceiverPhone))
                throw new Exception("Số điện thoại người nhận không được để trống.");

            if (string.IsNullOrWhiteSpace(order.ShippingAddress))
                throw new Exception("Địa chỉ giao hàng không được để trống.");

            if (orderDetails == null || orderDetails.Count == 0)
                throw new Exception("Đơn hàng phải có ít nhất 1 sản phẩm.");

            decimal totalAmount = 0;

            foreach (var detail in orderDetails)
            {
                if (detail.ProductId <= 0)
                    throw new Exception("Product ID không hợp lệ.");

                if (detail.Quantity <= 0)
                    throw new Exception("Số lượng phải lớn hơn 0.");

                var product = _productRepository.GetProductById(detail.ProductId);
                if (product == null)
                    throw new Exception($"Không tìm thấy sản phẩm có ID = {detail.ProductId}.");

                if (!product.Status)
                    throw new Exception($"Sản phẩm '{product.ProductName}' đang ngừng bán.");

                if (product.StockQuantity < detail.Quantity)
                    throw new Exception($"Sản phẩm '{product.ProductName}' không đủ số lượng trong kho.");

                detail.UnitPrice = product.Price;
                detail.LineTotal = product.Price * detail.Quantity;
                totalAmount += detail.LineTotal;
            }

            order.OrderDate = DateTime.Now;
            order.TotalAmount = totalAmount;
            order.Status = "Pending";
            order.ProcessedAt = null;
            order.PaidAt = null;

            _orderRepository.AddOrder(order);

            foreach (var detail in orderDetails)
            {
                detail.OrderId = order.OrderId;
            }

            _orderRepository.AddOrderDetails(orderDetails);

            foreach (var detail in orderDetails)
            {
                var product = _productRepository.GetProductById(detail.ProductId);
                if (product != null)
                {
                    int newQuantity = product.StockQuantity - detail.Quantity;
                    _productRepository.UpdateStockQuantity(product.ProductId, newQuantity);
                }
            }
        }

        public void UpdateOrderStatus(int orderId, string status)
        {
            if (orderId <= 0)
                throw new Exception("Order ID không hợp lệ.");

            if (string.IsNullOrWhiteSpace(status))
                throw new Exception("Trạng thái không được để trống.");

            status = status.Trim();

            var validStatuses = new List<string> { "Pending", "Processing", "Completed", "Cancelled" };
            if (!validStatuses.Contains(status))
                throw new Exception("Trạng thái không hợp lệ.");

            var order = _orderRepository.GetOrderById(orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn hàng.");

            if (string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(order.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Order đang ở trạng thái '{order.Status}' nên không thể thay đổi nữa.");
            }

            _orderRepository.UpdateOrderStatus(orderId, status);
        }

        public void CancelOrder(int orderId, int customerId)
        {
            if (orderId <= 0)
                throw new Exception("Order ID không hợp lệ.");

            if (customerId <= 0)
                throw new Exception("Customer ID không hợp lệ.");

            var order = _orderRepository.GetOrderById(orderId);

            if (order == null || order.CustomerId != customerId)
                throw new Exception("Không tìm thấy đơn hàng.");

            if (order.Status != "Pending")
                throw new Exception("Chỉ được hủy đơn hàng ở trạng thái Pending.");

            if (order.OrderDetails == null || order.OrderDetails.Count == 0)
                throw new Exception("Đơn hàng không có chi tiết.");

            foreach (var detail in order.OrderDetails)
            {
                var product = _productRepository.GetProductById(detail.ProductId);
                if (product != null)
                {
                    int newQuantity = product.StockQuantity + detail.Quantity;
                    _productRepository.UpdateStockQuantity(product.ProductId, newQuantity);
                }
            }

            _orderRepository.UpdateOrderStatus(orderId, "Cancelled");
        }
    }
}