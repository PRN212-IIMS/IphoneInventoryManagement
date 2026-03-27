using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services.Implementations;

namespace WPFApp.Views.Customer
{
    public partial class OrderHistoryView : UserControl
    {
        private readonly int _customerId;

        public OrderHistoryView(int customerId)
        {
            InitializeComponent();
            _customerId = customerId;
            LoadOrders();
        }

        private void LoadOrders()
        {
            var orderService = new OrderService();
            dgOrders.ItemsSource = orderService.GetOrdersByCustomerId(_customerId);
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
            else
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

                var orderService = new OrderService();
                orderService.CancelOrder(selected.OrderId, _customerId);
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