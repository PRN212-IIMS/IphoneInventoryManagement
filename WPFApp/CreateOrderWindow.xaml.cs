using BusinessObjects;
using Services;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WPFApp
{
    public partial class CreateOrderWindow : Window
    {
        private readonly OrderService _orderService;
        private readonly int _customerId;

        public CreateOrderWindow(int customerId)
        {
            InitializeComponent();
            _orderService = new OrderService();
            _customerId = customerId;

            LoadCart();
        }

        private void LoadCart()
        {
            dgCart.ItemsSource = null;
            dgCart.ItemsSource = CartStore.Items;

            txtSummaryCount.Text = $"Items: {CartStore.Items.Sum(x => x.Quantity)}";
            txtSummaryTotal.Text = $"Total Amount: {CartStore.GetTotalAmount():N0}";
        }

        private void RemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgCart.SelectedItem as CartItemViewModel;
            if (selected == null)
            {
                MessageBox.Show("Please select an item.");
                return;
            }

            CartStore.RemoveItem(selected);
            LoadCart();
        }

        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            CartStore.Clear();
            LoadCart();
        }

        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CartStore.Items.Any())
                {
                    MessageBox.Show("Cart is empty.");
                    return;
                }

                var order = new Order
                {
                    CustomerId = _customerId,
                    ReceiverName = txtReceiverName.Text.Trim(),
                    ReceiverPhone = txtReceiverPhone.Text.Trim(),
                    ShippingAddress = txtShippingAddress.Text.Trim()
                };

                var details = CartStore.Items.Select(x => new OrderDetail
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity
                }).ToList();

                _orderService.CreateOrder(order, details);

                MessageBox.Show("Order created successfully.");
                CartStore.Clear();
                LoadCart();

                txtReceiverName.Clear();
                txtReceiverPhone.Clear();
                txtShippingAddress.Clear();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ViewOrders_Click(object sender, RoutedEventArgs e)
        {
            OrderHistoryWindow window = new OrderHistoryWindow(_customerId);
            window.ShowDialog();
        }
    }
}