using BusinessObjects;
using Microsoft.VisualBasic.ApplicationServices;
using Services.Implementations;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WPFApp.ViewModels;
using CustomerEntity = BusinessObjects.Customer;

namespace WPFApp.Views.Staff
{
    public partial class CreateOrderWindow : Window
    {
        private const int MaxLineQuantity = 20;
        private const int MinReceiverNameLength = 2;
        private const int MaxReceiverNameLength = 100;
        private const int MinShippingAddressLength = 10;
        private const int MaxShippingAddressLength = 255;

        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly int _staffId;

        private readonly List<OrderCreateItemViewModel> _detailItems = new();

        public CreateOrderWindow(int staffId)
        {
            InitializeComponent();
            _orderService = new OrderService();
            _customerService = new CustomerService();
            _productService = new ProductService();
            _staffId = staffId;

            LoadCustomers();
            LoadProducts();
            RefreshDetailGrid();
            UpdateTotalAmount();
        }

        private void LoadCustomers()
        {
            cbCustomers.ItemsSource = _customerService.GetAllCustomers().ToList();
            cbCustomers.DisplayMemberPath = "FullName";
            cbCustomers.SelectedValuePath = "CustomerId";
            cbCustomers.SelectedIndex = -1;
            ///
        }

        private void LoadProducts()
        {
            cbProducts.ItemsSource = _productService.GetAllProducts()
                .Where(p => p.Status && p.StockQuantity > 0)
                .ToList();

            cbProducts.SelectedIndex = cbProducts.Items.Count > 0 ? 0 : -1;
        }

        private void RefreshDetailGrid()
        {
            dgDetails.ItemsSource = null;
            dgDetails.ItemsSource = _detailItems;
            dgDetails.Items.Refresh();
        }

        private void UpdateTotalAmount()
        {
            decimal total = _detailItems.Sum(x => x.LineTotal);
            txtTotalAmount.Text = $"Total Amount: {total:N0}";
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbProducts.SelectedItem is not Product selectedProduct)
                {
                    MessageBox.Show("Please select a product.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtQuantity.Text.Trim(), out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Quantity must be an integer greater than 0.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (quantity > MaxLineQuantity)
                {
                    MessageBox.Show($"Each product line cannot exceed {MaxLineQuantity} units per order.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (selectedProduct.StockQuantity < quantity)
                {
                    MessageBox.Show(
                        $"Insufficient stock for product '{selectedProduct.ProductName}'.",
                        "Validation",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var existing = _detailItems.FirstOrDefault(x => x.ProductId == selectedProduct.ProductId);
                if (existing != null)
                {
                    if (selectedProduct.StockQuantity < existing.Quantity + quantity)
                    {
                        MessageBox.Show(
                            $"Insufficient stock for product '{selectedProduct.ProductName}'.",
                            "Validation",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    existing.Quantity += quantity;
                    if (existing.Quantity > MaxLineQuantity)
                    {
                        existing.Quantity -= quantity;
                        MessageBox.Show($"Total quantity per product cannot exceed {MaxLineQuantity}.", "Validation",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    _detailItems.Add(new OrderCreateItemViewModel
                    {
                        ProductId = selectedProduct.ProductId,
                        ProductName = selectedProduct.ProductName,
                        Quantity = quantity,
                        UnitPrice = selectedProduct.Price
                    });
                }

                RefreshDetailGrid();
                UpdateTotalAmount();
                txtQuantity.Clear();
                cbProducts.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Add Item Failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is OrderCreateItemViewModel item)
            {
                _detailItems.Remove(item);
                RefreshDetailGrid();
                UpdateTotalAmount();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_detailItems.Count == 0)
                {
                    MessageBox.Show("The order must contain at least one product.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                CustomerEntity? selectedCustomer = cbCustomers.SelectedItem as CustomerEntity;
                string receiverName = txtReceiverName.Text.Trim();
                string receiverPhone = NormalizePhone(txtReceiverPhone.Text);
                string shippingAddress = txtShippingAddress.Text.Trim();

                string? validationMessage = ValidateOrderForm(receiverName, receiverPhone, shippingAddress);
                if (!string.IsNullOrWhiteSpace(validationMessage))
                {
                    MessageBox.Show(validationMessage, "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var order = new Order
                {
                    CustomerId = selectedCustomer?.CustomerId,
                    StaffId = _staffId,
                    ReceiverName = receiverName,
                    ReceiverPhone = receiverPhone,
                    ShippingAddress = shippingAddress
                };

                var details = _detailItems.Select(x => new OrderDetail
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity
                }).ToList();

                _orderService.CreateOrder(order, details);

                MessageBox.Show("Order created successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void cbCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbCustomers.SelectedItem == null)
                return;
            if (cbCustomers.SelectedItem is CustomerEntity selectedCustomer)
            {
                txtReceiverName.Text = selectedCustomer.FullName ?? "";
                txtReceiverPhone.Text = selectedCustomer.Phone ?? "";
            }
            else
            {
                txtReceiverName.Clear();
                txtReceiverPhone.Clear();
            }
        }
        private void btnClearCustomer_Click(object sender, RoutedEventArgs e)
        {
            cbCustomers.SelectedIndex = -1;
            txtReceiverName.Text = "";
            txtReceiverPhone.Text = "";
            txtShippingAddress.Text = "";
            cbCustomers.Focus();
        }

        private static string? ValidateOrderForm(string receiverName, string receiverPhone, string shippingAddress)
        {
            if (string.IsNullOrWhiteSpace(receiverName))
                return "Receiver name cannot be empty.";

            if (receiverName.Length < MinReceiverNameLength || receiverName.Length > MaxReceiverNameLength)
                return $"Receiver name must be between {MinReceiverNameLength} and {MaxReceiverNameLength} characters.";

            if (!Regex.IsMatch(receiverName, @"^[\p{L}\s'\.\-]+$"))
                return "Receiver name contains invalid characters.";

            if (string.IsNullOrWhiteSpace(receiverPhone))
                return "Receiver phone cannot be empty.";

            if (!Regex.IsMatch(receiverPhone, @"^(0\d{9}|\+84\d{9})$"))
                return "Receiver phone number is invalid.";

            if (string.IsNullOrWhiteSpace(shippingAddress))
                return "Shipping address cannot be empty.";

            if (shippingAddress.Length < MinShippingAddressLength || shippingAddress.Length > MaxShippingAddressLength)
                return $"Shipping address must be between {MinShippingAddressLength} and {MaxShippingAddressLength} characters.";

            return null;
        }

        private static string NormalizePhone(string phone)
        {
            return (phone ?? string.Empty).Trim().Replace(" ", string.Empty).Replace(".", string.Empty);
        }
    }
}