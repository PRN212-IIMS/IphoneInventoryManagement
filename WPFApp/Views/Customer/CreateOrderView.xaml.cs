using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services.Implementations;

namespace WPFApp.Views.Customer
{
    public partial class CreateOrderView : UserControl
    {
        private readonly OrderService _orderService;
        private readonly int _customerId;
        private readonly Action _openOrders;

        public CreateOrderView(int customerId, Action openOrders)
        {
            InitializeComponent();
            _orderService = new OrderService();
            _customerId = customerId;
            _openOrders = openOrders;

            LoadCart();
        }

        private void LoadCart()
        {
            dgCart.ItemsSource = null;
            dgCart.ItemsSource = CartStore.Items;

            txtSummaryCount.Text = $"Items: {CartStore.Items.Sum(x => x.Quantity)}";
            txtSummaryTotal.Text = $"Total Amount: {CartStore.GetTotalAmount():N0}";
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

            if (!Regex.IsMatch(receiverName, @"^[\p{L}\s]+$"))
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

            if (!Regex.IsMatch(receiverPhone, @"^0\d{9,10}$"))
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

                _openOrders?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void dgCart_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (e.Row.Item is not CartItemViewModel item)
                    return;

                if (e.EditingElement is TextBox tb)
                {
                    if (!int.TryParse(tb.Text, out int qty) || qty <= 0)
                    {
                        MessageBox.Show("Quantity must be greater than 0.");
                        item.Quantity = 1;
                    }
                    else
                    {
                        item.Quantity = qty;
                    }
                }

                dgCart.ItemsSource = null;
                dgCart.ItemsSource = CartStore.Items;

                txtSummaryCount.Text = $"Items: {CartStore.Items.Sum(x => x.Quantity)}";
                txtSummaryTotal.Text = $"Total Amount: {CartStore.GetTotalAmount():N0}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}