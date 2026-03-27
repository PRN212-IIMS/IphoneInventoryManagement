using BusinessObjects;
using Microsoft.VisualBasic.ApplicationServices;
using Services.Implementations;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WPFApp.ViewModels;
using CustomerEntity = BusinessObjects.Customer;

namespace WPFApp.Views.Staff
{
    public partial class CreateOrderWindow : Window
    {
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
                    MessageBox.Show("Vui lòng chọn sản phẩm.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtQuantity.Text.Trim(), out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Quantity phải là số nguyên > 0.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (selectedProduct.StockQuantity < quantity)
                {
                    MessageBox.Show(
                        $"Sản phẩm '{selectedProduct.ProductName}' không đủ số lượng trong kho.",
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
                            $"Sản phẩm '{selectedProduct.ProductName}' không đủ số lượng trong kho.",
                            "Validation",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    existing.Quantity += quantity;
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
                    MessageBox.Show("Đơn hàng phải có ít nhất 1 sản phẩm.", "Validation",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                CustomerEntity? selectedCustomer = cbCustomers.SelectedItem as CustomerEntity;

                var order = new Order
                {
                    CustomerId = selectedCustomer?.CustomerId,
                    StaffId = _staffId,
                    ReceiverName = txtReceiverName.Text.Trim(),
                    ReceiverPhone = txtReceiverPhone.Text.Trim(),
                    ShippingAddress = txtShippingAddress.Text.Trim()
                };

                var details = _detailItems.Select(x => new OrderDetail
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity
                }).ToList();

                _orderService.CreateOrder(order, details);

                MessageBox.Show("Tạo đơn hàng thành công.", "Success",
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
    }
}