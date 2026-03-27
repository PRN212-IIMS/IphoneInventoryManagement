using BusinessObjects;
using Services.Implementations;
using System.Windows;
using System.Windows.Controls;

namespace WPFApp.Views.Customer
{
    public partial class OrderHistoryWindow : Window
    {
        private readonly OrderService _orderService;
        private readonly int _customerId;

        public OrderHistoryWindow(int customerId)
        {
            InitializeComponent();
            _orderService = new OrderService();
            _customerId = customerId;

            LoadOrders();
        }

        private void LoadOrders()
        {
            var freshService = new OrderService();
            dgOrders.ItemsSource = freshService.GetOrdersByCustomerId(_customerId);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void ViewDetail_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgOrders.SelectedItem as Order;
            if (selected == null)
            {
                MessageBox.Show("Please select an order.");
                return;
            }

            OrderDetailWindow window = new OrderDetailWindow(_customerId, selected.OrderId);
            bool? result = window.ShowDialog();

            if (result == true)
            {
                LoadOrders();
            }
        }

        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selected = dgOrders.SelectedItem as Order;
                if (selected == null)
                {
                    MessageBox.Show("Please select an order.");
                    return;
                }

                _orderService.CancelOrder(selected.OrderId, _customerId);
                MessageBox.Show("Order cancelled successfully.");
                LoadOrders();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}