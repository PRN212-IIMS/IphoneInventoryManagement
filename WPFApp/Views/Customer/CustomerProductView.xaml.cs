using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services.Implementations;

namespace WPFApp.Views.Customer
{
    public partial class CustomerProductView : UserControl
    {
        private readonly ProductService _productService;
        private readonly int _customerId;
        private readonly Action _openCheckout;

        public CustomerProductView(int customerId, Action openCheckout)
        {
            InitializeComponent();
            _productService = new ProductService();
            _customerId = customerId;
            _openCheckout = openCheckout;

            LoadFilters();
            LoadProducts();
            UpdateCartStatus();
        }

        private void LoadFilters()
        {
            var products = _productService.GetAllProducts();

            cbColor.Items.Clear();
            cbColor.Items.Add("All");
            foreach (var color in products.Where(p => !string.IsNullOrWhiteSpace(p.Color))
                                          .Select(p => p.Color!)
                                          .Distinct()
                                          .OrderBy(x => x))
            {
                cbColor.Items.Add(color);
            }
            cbColor.SelectedIndex = 0;

            cbStorage.Items.Clear();
            cbStorage.Items.Add("All");
            foreach (var storage in products.Where(p => !string.IsNullOrWhiteSpace(p.StorageCapacity))
                                            .Select(p => p.StorageCapacity!)
                                            .Distinct()
                                            .OrderBy(x => x))
            {
                cbStorage.Items.Add(storage);
            }
            cbStorage.SelectedIndex = 0;
        }

        private void LoadProducts()
        {
            dgProducts.ItemsSource = _productService.GetAllProducts()
                .Where(p => p.Status && p.StockQuantity > 0)
                .ToList();
        }

        private void UpdateCartStatus()
        {
            txtCartStatus.Text = $"Cart: {CartStore.Items.Count} item(s) | Total: {CartStore.GetTotalAmount():N0}";
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            string? color = cbColor.SelectedItem?.ToString();
            string? storage = cbStorage.SelectedItem?.ToString();

            if (color == "All") color = null;
            if (storage == "All") storage = null;

            var filtered = _productService
                .FilterProducts(color, storage, true)
                .Where(p => p.StockQuantity > 0)
                .ToList();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                filtered = filtered.Where(p =>
                        p.ProductName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrWhiteSpace(p.Model) && p.Model.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            dgProducts.ItemsSource = filtered;
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cbColor.SelectedIndex = 0;
            cbStorage.SelectedIndex = 0;
            txtQuantity.Text = "1";
            LoadProducts();
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Product? selected = dgProducts.SelectedItem as Product;
                if (selected == null)
                {
                    MessageBox.Show("Please select a product.");
                    return;
                }

                if (!int.TryParse(txtQuantity.Text.Trim(), out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Quantity must be a positive integer.");
                    return;
                }

                if (quantity > selected.StockQuantity)
                {
                    MessageBox.Show("Quantity exceeds stock.");
                    return;
                }

                CartStore.AddItem(new CartItemViewModel
                {
                    ProductId = selected.ProductId,
                    ProductName = selected.ProductName,
                    Model = selected.Model,
                    Color = selected.Color,
                    StorageCapacity = selected.StorageCapacity,
                    Quantity = quantity,
                    UnitPrice = selected.Price,
                    LineTotal = selected.Price * quantity
                });

                UpdateCartStatus();
                MessageBox.Show("Added to cart successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OpenCheckout_Click(object sender, RoutedEventArgs e)
        {
            _openCheckout?.Invoke();
        }
    }
}