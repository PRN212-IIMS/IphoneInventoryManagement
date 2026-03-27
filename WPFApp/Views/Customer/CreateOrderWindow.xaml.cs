using BusinessObjects;
using Services.Implementations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WPFApp.Views.Customer
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

                if (!ValidateShippingInfo())
                {
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
        private bool ValidateShippingInfo()
        {
            string receiverName = txtReceiverName.Text.Trim();
            string receiverPhone = txtReceiverPhone.Text.Trim();
            string shippingAddress = txtShippingAddress.Text.Trim();

            if (string.IsNullOrWhiteSpace(receiverName))
            {
                MessageBox.Show("Receiver name must not be empty.");
                txtReceiverName.Focus();
                return false;
            }

            if (receiverName.Length < 2)
            {
                MessageBox.Show("Receiver name must be at least 2 characters.");
                txtReceiverName.Focus();
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(receiverName, @"^[\p{L}\s]+$"))
            {
                MessageBox.Show("Receiver name must contain letters only.");
                txtReceiverName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(receiverPhone))
            {
                MessageBox.Show("Receiver phone must not be empty.");
                txtReceiverPhone.Focus();
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(receiverPhone, @"^0\d{9,10}$"))
            {
                MessageBox.Show("Receiver phone must start with 0 and contain 10 to 11 digits.");
                txtReceiverPhone.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(shippingAddress))
            {
                MessageBox.Show("Shipping address must not be empty.");
                txtShippingAddress.Focus();
                return false;
            }

            if (shippingAddress.Length < 5)
            {
                MessageBox.Show("Shipping address must be at least 5 characters.");
                txtShippingAddress.Focus();
                return false;
            }

            return true;
        }
    }
}