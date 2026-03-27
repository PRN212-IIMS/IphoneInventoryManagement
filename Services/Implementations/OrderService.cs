using BusinessObjects;
using Repositories.Interfaces;
using Services.Interfaces;
using Repositories.Implementations;
using System.Text.RegularExpressions;

namespace Services.Implementations
{
    public class OrderService : IOrderService
    {
        private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Pending", "Processing", "Completed", "Cancelled"
        };

        private const int MaxOrderLineQuantity = 20;
        private const int MaxDistinctProductsPerOrder = 20;
        private const int MinReceiverNameLength = 2;
        private const int MaxReceiverNameLength = 100;
        private const int MinShippingAddressLength = 10;
        private const int MaxShippingAddressLength = 255;

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
            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.Trim();
                if (!ValidStatuses.Contains(status))
                    throw new Exception("Invalid filter status.");
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
                throw new Exception("Invalid date range: start date must be on or before end date.");

            return _orderRepository.FilterOrders(status, fromDate, toDate);
        }

        public void CreateOrder(Order order, List<OrderDetail> orderDetails)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            bool hasStaff = order.StaffId != null && order.StaffId > 0;
            bool hasCustomer = order.CustomerId != null && order.CustomerId > 0;

            if (!hasStaff && !hasCustomer)
                throw new Exception("Order must be created by a staff member or customer.");

            if (string.IsNullOrWhiteSpace(order.ReceiverName))
                throw new Exception("Receiver name cannot be empty.");
            order.ReceiverName = order.ReceiverName.Trim();
            if (order.ReceiverName.Length < MinReceiverNameLength || order.ReceiverName.Length > MaxReceiverNameLength)
                throw new Exception($"Receiver name must be between {MinReceiverNameLength} and {MaxReceiverNameLength} characters.");
            if (!Regex.IsMatch(order.ReceiverName, @"^[\p{L}\s'\.\-]+$"))
                throw new Exception("Receiver name contains invalid characters.");

            if (string.IsNullOrWhiteSpace(order.ReceiverPhone))
                throw new Exception("Receiver phone cannot be empty.");
            order.ReceiverPhone = NormalizePhone(order.ReceiverPhone);
            if (!IsValidVietnamPhone(order.ReceiverPhone))
                throw new Exception("Receiver phone number is invalid.");

            if (string.IsNullOrWhiteSpace(order.ShippingAddress))
                throw new Exception("Shipping address cannot be empty.");
            order.ShippingAddress = order.ShippingAddress.Trim();
            if (order.ShippingAddress.Length < MinShippingAddressLength || order.ShippingAddress.Length > MaxShippingAddressLength)
                throw new Exception($"Shipping address must be between {MinShippingAddressLength} and {MaxShippingAddressLength} characters.");

            if (orderDetails == null || orderDetails.Count == 0)
                throw new Exception("The order must contain at least one product.");
            if (orderDetails.Count > MaxDistinctProductsPerOrder)
                throw new Exception($"An order cannot have more than {MaxDistinctProductsPerOrder} line items.");

            decimal totalAmount = 0;
            var productIds = new HashSet<int>();

            foreach (var detail in orderDetails)
            {
                if (detail.ProductId <= 0)
                    throw new Exception("Invalid product ID.");
                if (!productIds.Add(detail.ProductId))
                    throw new Exception("Duplicate product in order; please merge quantities.");

                if (detail.Quantity <= 0)
                    throw new Exception("Quantity must be greater than 0.");
                if (detail.Quantity > MaxOrderLineQuantity)
                    throw new Exception($"Each product line cannot exceed {MaxOrderLineQuantity} units per order.");

                var product = _productRepository.GetProductById(detail.ProductId);
                if (product == null)
                    throw new Exception($"Product with ID {detail.ProductId} was not found.");

                if (!product.Status)
                    throw new Exception($"Product '{product.ProductName}' is not available for sale.");

                if (product.StockQuantity < detail.Quantity)
                    throw new Exception($"Insufficient stock for product '{product.ProductName}'.");

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
                throw new Exception("Invalid order ID.");

            if (string.IsNullOrWhiteSpace(status))
                throw new Exception("Status cannot be empty.");

            status = status.Trim();
            if (!ValidStatuses.Contains(status))
                throw new Exception("Invalid status.");

            var order = _orderRepository.GetOrderById(orderId);
            if (order == null)
                throw new Exception("Order not found.");

            string currentStatus = (order.Status ?? string.Empty).Trim();
            if (string.Equals(currentStatus, status, StringComparison.OrdinalIgnoreCase))
                throw new Exception("New status is the same as the current status.");

            ValidateStatusTransition(currentStatus, status);

            _orderRepository.UpdateOrderStatus(orderId, status);
        }

        public void CancelOrder(int orderId, int customerId)
        {
            if (orderId <= 0)
                throw new Exception("Invalid order ID.");

            if (customerId <= 0)
                throw new Exception("Invalid customer ID.");

            var order = _orderRepository.GetOrderById(orderId);

            if (order == null || order.CustomerId != customerId)
                throw new Exception("Order not found.");

            if (order.Status != "Pending")
                throw new Exception("Only orders in Pending status can be cancelled.");

            if (order.OrderDetails == null || order.OrderDetails.Count == 0)
                throw new Exception("Order has no line items.");

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

        private static void ValidateStatusTransition(string currentStatus, string newStatus)
        {
            if (!ValidStatuses.Contains(currentStatus))
                throw new Exception("Current order status is invalid.");

            bool isValidTransition =
                (string.Equals(currentStatus, "Pending", StringComparison.OrdinalIgnoreCase)
                    && (string.Equals(newStatus, "Processing", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(newStatus, "Cancelled", StringComparison.OrdinalIgnoreCase)))
                || (string.Equals(currentStatus, "Processing", StringComparison.OrdinalIgnoreCase)
                    && (string.Equals(newStatus, "Completed", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(newStatus, "Cancelled", StringComparison.OrdinalIgnoreCase)));

            if (!isValidTransition)
                throw new Exception($"Cannot change status from '{currentStatus}' to '{newStatus}'.");
        }

        private static string NormalizePhone(string phone)
        {
            return phone.Trim().Replace(" ", string.Empty).Replace(".", string.Empty);
        }

        private static bool IsValidVietnamPhone(string phone)
        {
            return Regex.IsMatch(phone, @"^(0\d{9}|\+84\d{9})$");
        }
    }
}
