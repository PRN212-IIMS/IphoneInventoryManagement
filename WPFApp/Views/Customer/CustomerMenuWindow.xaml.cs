using System.Windows;

namespace WPFApp.Views.Customer
{
    public partial class CustomerMenuWindow : Window
    {
        private const int CurrentCustomerId = 1;

        public CustomerMenuWindow()
        {
            InitializeComponent();
        }

        private void OpenProducts_Click(object sender, RoutedEventArgs e)
        {
            CustomerProductWindow window = new CustomerProductWindow(CurrentCustomerId);
            window.ShowDialog();
        }

        private void OpenCheckout_Click(object sender, RoutedEventArgs e)
        {
            CreateOrderWindow window = new CreateOrderWindow(CurrentCustomerId);
            window.ShowDialog();
        }

        private void OpenOrders_Click(object sender, RoutedEventArgs e)
        {
            OrderHistoryWindow window = new OrderHistoryWindow(CurrentCustomerId);
            window.ShowDialog();
        }
    }
}