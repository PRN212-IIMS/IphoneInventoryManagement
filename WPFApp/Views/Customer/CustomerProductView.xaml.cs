using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects;

namespace WPFApp.Views.Customer
{
    public partial class CustomerProductView : UserControl
    {
        private readonly int _customerId;
        private readonly Action _openCheckout;
        private List<object> _allProducts = new();

        public CustomerProductView(int customerId, Action openCheckout)
        {
            InitializeComponent();
            _customerId = customerId;
            _openCheckout = openCheckout;

            LoadFilterOptions();
            LoadProducts();
            UpdateCartStatus();
        }

        private void LoadFilterOptions()
        {
            cbColor.Items.Clear();
            cbStorage.Items.Clear();

            cbColor.Items.Add("All");
            cbStorage.Items.Add("All");

            cbColor.SelectedIndex = 0;
            cbStorage.SelectedIndex = 0;
        }

        private void LoadProducts()
        {
            _allProducts = GetProductsFromService();
            dgProducts.ItemsSource = _allProducts;
            RefreshFilterOptionsFromProducts();
        }

        private List<object> GetProductsFromService()
        {
            try
            {
                var serviceType = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType("Services.Implementations.ProductService", false))
                    .FirstOrDefault(t => t != null);

                if (serviceType == null)
                {
                    return new List<object>();
                }

                var service = Activator.CreateInstance(serviceType!);
                if (service == null)
                {
                    return new List<object>();
                }

                var method = serviceType!.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m =>
                        m.GetParameters().Length == 0 &&
                        (string.Equals(m.Name, "GetProducts", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(m.Name, "GetAllProducts", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(m.Name, "GetProduct", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(m.Name, "GetAll", StringComparison.OrdinalIgnoreCase)));

                if (method == null)
                {
                    return new List<object>();
                }

                var result = method.Invoke(service, null);
                if (result is IEnumerable enumerable)
                {
                    return enumerable.Cast<object>().ToList();
                }
            }
            catch
            {
            }

            return new List<object>();
        }

        private void RefreshFilterOptionsFromProducts()
        {
            var colors = _allProducts
                .Select(x => GetStringValue(x, "Color"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var storages = _allProducts
                .Select(x => GetStringValue(x, "StorageCapacity", "Storage"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

            var selectedColor = cbColor.SelectedItem?.ToString() ?? "All";
            var selectedStorage = cbStorage.SelectedItem?.ToString() ?? "All";

            cbColor.Items.Clear();
            cbStorage.Items.Clear();

            cbColor.Items.Add("All");
            cbStorage.Items.Add("All");

            foreach (var color in colors)
            {
                cbColor.Items.Add(color);
            }

            foreach (var storage in storages)
            {
                cbStorage.Items.Add(storage);
            }

            cbColor.SelectedItem = cbColor.Items.Contains(selectedColor) ? selectedColor : "All";
            cbStorage.SelectedItem = cbStorage.Items.Contains(selectedStorage) ? selectedStorage : "All";
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<object> query = _allProducts;

            string keyword = txtSearch.Text?.Trim() ?? string.Empty;
            string selectedColor = cbColor.SelectedItem?.ToString() ?? "All";
            string selectedStorage = cbStorage.SelectedItem?.ToString() ?? "All";
            int requestedQty = ParseInt(txtQuantity.Text, 1);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p =>
                    ContainsIgnoreCase(GetStringValue(p, "ProductName", "Name"), keyword)
                    || ContainsIgnoreCase(GetStringValue(p, "Model"), keyword)
                    || ContainsIgnoreCase(GetStringValue(p, "Color"), keyword)
                    || ContainsIgnoreCase(GetStringValue(p, "StorageCapacity", "Storage"), keyword));
            }

            if (!string.Equals(selectedColor, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => string.Equals(GetStringValue(p, "Color"), selectedColor, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.Equals(selectedStorage, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(p => string.Equals(GetStringValue(p, "StorageCapacity", "Storage"), selectedStorage, StringComparison.OrdinalIgnoreCase));
            }

            if (requestedQty > 0)
            {
                query = query.Where(p => GetIntValue(p, "StockQuantity", "Quantity", "UnitsInStock") >= requestedQty);
            }

            dgProducts.ItemsSource = query.ToList();
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            txtQuantity.Text = "1";
            cbColor.SelectedIndex = 0;
            cbStorage.SelectedIndex = 0;
            dgProducts.ItemsSource = _allProducts;
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selected = dgProducts.SelectedItem;
                if (selected == null)
                {
                    MessageBox.Show("Please select a product.");
                    return;
                }

                int quantity = ParseInt(txtQuantity.Text, 1);
                if (quantity <= 0)
                {
                    MessageBox.Show("Quantity must be greater than 0.");
                    return;
                }

                int stock = GetIntValue(selected, "StockQuantity", "Quantity", "UnitsInStock");
                if (stock <= 0)
                {
                    MessageBox.Show("This product is out of stock.");
                    return;
                }

                if (quantity > stock)
                {
                    MessageBox.Show("Quantity exceeds available stock.");
                    return;
                }

                int productId = GetIntValue(selected, "ProductId", "Id");
                if (productId <= 0)
                {
                    MessageBox.Show("Invalid product.");
                    return;
                }

                var cartItem = new CartItemViewModel
                {
                    ProductId = productId,
                    ProductName = GetStringValue(selected, "ProductName", "Name"),
                    Model = GetStringValue(selected, "Model"),
                    Color = GetStringValue(selected, "Color"),
                    StorageCapacity = GetStringValue(selected, "StorageCapacity", "Storage"),
                    Quantity = quantity,
                    UnitPrice = GetDecimalValue(selected, "Price", "UnitPrice")
                };

                CartStore.AddItem(cartItem);
                UpdateCartStatus();
                MessageBox.Show("Product added to cart.");
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

        private void UpdateCartStatus()
        {
            int itemCount = 0;
            try
            {
                itemCount = CartStore.Items.Sum(x => x.Quantity);
            }
            catch
            {
            }

            txtCartStatus.Text = $"Cart: {itemCount} item(s)";
        }

        private static bool ContainsIgnoreCase(string source, string keyword)
        {
            return source?.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetStringValue(object obj, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var prop = obj.GetType().GetProperty(propertyName);
                if (prop != null)
                {
                    var value = prop.GetValue(obj)?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }

            return string.Empty;
        }

        private static int GetIntValue(object obj, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var prop = obj.GetType().GetProperty(propertyName);
                if (prop == null) continue;

                var raw = prop.GetValue(obj);
                if (raw == null) continue;

                if (raw is int i) return i;
                if (int.TryParse(raw.ToString(), out var parsed)) return parsed;
            }

            return 0;
        }

        private static decimal GetDecimalValue(object obj, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var prop = obj.GetType().GetProperty(propertyName);
                if (prop == null) continue;

                var raw = prop.GetValue(obj);
                if (raw == null) continue;

                if (raw is decimal d) return d;
                if (decimal.TryParse(raw.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)) return parsed;
                if (decimal.TryParse(raw.ToString(), out parsed)) return parsed;
            }

            return 0;
        }

        private static int ParseInt(string? text, int defaultValue)
        {
            return int.TryParse(text, out var value) ? value : defaultValue;
        }
    }
}
