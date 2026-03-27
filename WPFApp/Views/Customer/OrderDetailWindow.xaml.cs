using System.Windows;
using BusinessObjects;
using Services.Implementations;

namespace WPFApp.Views.Customer
{
    public partial class OrderDetailWindow : Window
    {
        private readonly OrderService _orderService;
        private readonly int _customerId;
        private readonly int _orderId;

        public OrderDetailWindow(int customerId, int orderId)
        {
            InitializeComponent();
            _orderService = new OrderService();
            _customerId = customerId;
            _orderId = orderId;

            LoadOrderDetail();
        }

        private void LoadOrderDetail()
        {
            Order? order = _orderService.GetOrderById(_orderId);
            if (order == null)
            {
                MessageBox.Show("Order not found.");
                Close();
                return;
            }

            txtOrderId.Text = $"Order ID: {order.OrderId}";
            txtReceiver.Text = $"Receiver: {order.ReceiverName}";
            txtPhone.Text = $"Phone: {order.ReceiverPhone}";
            txtAddress.Text = $"Address: {order.ShippingAddress}";
            txtStatus.Text = $"Status: {order.Status}";
            txtOrderDate.Text = $"Order Date: {order.OrderDate}";
            txtTotalAmount.Text = $"Total Amount: {order.TotalAmount:N0}";

            dgOrderDetails.ItemsSource = order.OrderDetails;

            btnCancelOrder.Visibility = order.Status == "Pending"
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _orderService.CancelOrder(_orderId, _customerId);
                MessageBox.Show("Order cancelled successfully.");
                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}