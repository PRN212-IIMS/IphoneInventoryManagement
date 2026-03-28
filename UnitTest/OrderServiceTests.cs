using BusinessObjects;
using Services.Implementations;
using Xunit;

namespace UnitTest
{
    public class OrderServiceTests
    {
        private readonly FakeOrderRepository _fakeOrderRepository;
        private readonly FakeProductRepository _fakeProductRepository;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _fakeOrderRepository = new FakeOrderRepository();
            _fakeProductRepository = new FakeProductRepository();

            _fakeProductRepository.Products.Add(new Product
            {
                ProductId = 1,
                StaffId = 1,
                ProductName = "iPhone 14",
                Model = "A2890",
                Color = "Blue",
                StorageCapacity = "128GB",
                Price = 20000000,
                StockQuantity = 10,
                UrlImages = "https://example.com/iphone14.jpg",
                Status = true
            });

            _orderService = new OrderService(_fakeOrderRepository, _fakeProductRepository);
        }

        private Order CreateValidOrder()
        {
            return new Order
            {
                CustomerId = 1,
                ReceiverName = "Nguyen Van A",
                ReceiverPhone = "0912345678",
                ShippingAddress = "123 Nguyen Trai, Ha Noi"
            };
        }

        [Fact]
        public void CreateOrder_NoStaffAndNoCustomer_ThrowsException()
        {
            var order = new Order
            {
                ReceiverName = "Nguyen Van A",
                ReceiverPhone = "0912345678",
                ShippingAddress = "123 Nguyen Trai, Ha Noi"
            };

            var details = new List<OrderDetail>
            {
                new OrderDetail { ProductId = 1, Quantity = 1 }
            };

            var ex = Assert.Throws<Exception>(() => _orderService.CreateOrder(order, details));

            Assert.Equal("Order must be created by a staff member or customer.", ex.Message);
        }

        [Fact]
        public void CreateOrder_NoOrderDetails_ThrowsException()
        {
            var order = CreateValidOrder();

            var ex = Assert.Throws<Exception>(() => _orderService.CreateOrder(order, new List<OrderDetail>()));

            Assert.Equal("The order must contain at least one product.", ex.Message);
        }

        [Fact]
        public void CreateOrder_ValidOrder_SetsPendingAndUpdatesStock()
        {
            var order = CreateValidOrder();
            var details = new List<OrderDetail>
            {
                new OrderDetail { ProductId = 1, Quantity = 2 }
            };

            _orderService.CreateOrder(order, details);

            Assert.Equal(1, _fakeOrderRepository.Orders.Count);
            Assert.Equal("Pending", _fakeOrderRepository.Orders[0].Status);
            Assert.Equal(40000000, _fakeOrderRepository.Orders[0].TotalAmount);
            Assert.Equal(8, _fakeProductRepository.GetProductById(1)!.StockQuantity);
        }

        [Fact]
        public void UpdateOrderStatus_FromPendingToProcessing_UpdatesSuccessfully()
        {
            _fakeOrderRepository.Orders.Add(new Order
            {
                OrderId = 1,
                CustomerId = 1,
                ReceiverName = "Nguyen Van A",
                ReceiverPhone = "0912345678",
                ShippingAddress = "123 Nguyen Trai, Ha Noi",
                Status = "Pending",
                OrderDate = DateTime.Now
            });

            _orderService.UpdateOrderStatus(1, "Processing");

            Assert.Equal("Processing", _fakeOrderRepository.GetOrderById(1)!.Status);
        }

        [Fact]
        public void UpdateOrderStatus_FromCancelledToProcessing_ThrowsException()
        {
            _fakeOrderRepository.Orders.Add(new Order
            {
                OrderId = 1,
                CustomerId = 1,
                ReceiverName = "Nguyen Van A",
                ReceiverPhone = "0912345678",
                ShippingAddress = "123 Nguyen Trai, Ha Noi",
                Status = "Cancelled",
                OrderDate = DateTime.Now
            });

            var ex = Assert.Throws<Exception>(() => _orderService.UpdateOrderStatus(1, "Processing"));

            Assert.Equal("Cannot change status from 'Cancelled' to 'Processing'.", ex.Message);
        }

        [Fact]
        public void CancelOrder_PendingOrder_RestoresStockAndSetsCancelled()
        {
            _fakeOrderRepository.Orders.Add(new Order
            {
                OrderId = 1,
                CustomerId = 1,
                ReceiverName = "Nguyen Van A",
                ReceiverPhone = "0912345678",
                ShippingAddress = "123 Nguyen Trai, Ha Noi",
                Status = "Pending",
                OrderDate = DateTime.Now
            });

            _fakeOrderRepository.OrderDetails.Add(new OrderDetail
            {
                OrderId = 1,
                ProductId = 1,
                Quantity = 2,
                UnitPrice = 20000000,
                LineTotal = 40000000
            });

            _fakeProductRepository.UpdateStockQuantity(1, 8);

            _orderService.CancelOrder(1, 1);

            Assert.Equal("Cancelled", _fakeOrderRepository.GetOrderById(1)!.Status);
            Assert.Equal(10, _fakeProductRepository.GetProductById(1)!.StockQuantity);
        }
    }
}