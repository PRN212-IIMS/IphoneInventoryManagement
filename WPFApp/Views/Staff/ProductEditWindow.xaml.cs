using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using BusinessObjects;
using Services.Implementations;
using Services.Interfaces;

namespace WPFApp.Views.Staff
{
    public partial class ProductEditWindow : Window
    {
        private static readonly HashSet<string> AllowedStorageCapacities = new(StringComparer.OrdinalIgnoreCase)
        {
            "64GB", "128GB", "256GB", "512GB", "1TB"
        };

        private const decimal MinPrice = 1000000m;
        private const decimal MaxPrice = 100000000m;
        private const int MaxStockQuantity = 10000;
        private const int MinProductNameLength = 3;
        private const int MaxProductNameLength = 120;
        private const int MinModelLength = 1;
        private const int MaxModelLength = 60;
        private const int MaxColorLength = 30;

        private static readonly Regex ModelPattern = new(@"^[\p{L}\d\s\-]+$", RegexOptions.Compiled);
        private static readonly Regex ColorPattern = new(@"^[\p{L}\d\s\-]+$", RegexOptions.Compiled);
        private static readonly Regex PriceDigitsOnly = new(@"^\d+$", RegexOptions.Compiled);

        private readonly IProductService _productService;
        private readonly Product? _editingProduct;
        private readonly int _staffId;

        public ProductEditWindow(int staffId)
        {
            InitializeComponent();
            _productService = new ProductService();
            _editingProduct = null;
            _staffId = staffId;
            txtTitle.Text = "Add Product";
        }

        public ProductEditWindow(Product product, int staffId)
        {
            InitializeComponent();
            _productService = new ProductService();
            _editingProduct = product;
            _staffId = staffId;

            txtTitle.Text = "Edit Product";
            LoadProductData(product);
        }

        private void LoadProductData(Product product)
        {
            txtProductName.Text = product.ProductName;
            txtModel.Text = product.Model;
            txtColor.Text = product.Color;
            txtStorage.Text = product.StorageCapacity;
            txtPrice.Text = decimal.Truncate(product.Price).ToString(CultureInfo.InvariantCulture);
            txtStockQuantity.Text = product.StockQuantity.ToString();
            txtImageUrl.Text = product.UrlImages;
            chkStatus.IsChecked = product.Status;
        }

        private bool TryValidateInputs(out string errorMessage, out decimal price, out int stockQuantity)
        {
            price = 0;
            stockQuantity = 0;

            string productName = txtProductName.Text.Trim();
            if (string.IsNullOrWhiteSpace(productName))
            {
                errorMessage = "Product name cannot be empty.";
                return false;
            }

            if (productName.Length < MinProductNameLength || productName.Length > MaxProductNameLength)
            {
                errorMessage = $"Product name must be between {MinProductNameLength} and {MaxProductNameLength} characters.";
                return false;
            }

            string model = txtModel.Text.Trim();
            if (string.IsNullOrWhiteSpace(model))
            {
                errorMessage = "Model cannot be empty.";
                return false;
            }

            if (model.Length < MinModelLength || model.Length > MaxModelLength)
            {
                errorMessage = $"Model must be between {MinModelLength} and {MaxModelLength} characters.";
                return false;
            }

            if (!ModelPattern.IsMatch(model))
            {
                errorMessage = "Model cannot contain special characters (only letters, digits, spaces, and hyphens).";
                return false;
            }

            string color = txtColor.Text.Trim();
            if (string.IsNullOrWhiteSpace(color))
            {
                errorMessage = "Color cannot be empty.";
                return false;
            }

            if (color.Length > MaxColorLength)
            {
                errorMessage = $"Color cannot exceed {MaxColorLength} characters.";
                return false;
            }

            if (!ColorPattern.IsMatch(color))
            {
                errorMessage = "Color may only contain letters, digits, spaces, or hyphens.";
                return false;
            }

            string storage = txtStorage.Text.Trim();
            if (string.IsNullOrWhiteSpace(storage))
            {
                errorMessage = "Storage capacity cannot be empty.";
                return false;
            }

            storage = storage.ToUpperInvariant();
            if (!AllowedStorageCapacities.Contains(storage))
            {
                errorMessage = "Invalid storage capacity. Allowed values: 64GB, 128GB, 256GB, 512GB, 1TB.";
                return false;
            }

            string priceText = txtPrice.Text.Trim();
            if (string.IsNullOrWhiteSpace(priceText))
            {
                errorMessage = "Price cannot be empty.";
                return false;
            }

            if (!decimal.TryParse(priceText, NumberStyles.Number, CultureInfo.InvariantCulture, out price))
            {
                errorMessage = "Price must be a valid number (digits only, no commas or decimal separators).";
                return false;
            }

            string priceInvariant = price.ToString(CultureInfo.InvariantCulture);
            if (!PriceDigitsOnly.IsMatch(priceInvariant))
            {
                errorMessage = "Price must be a whole number with digits 0-9 only.";
                return false;
            }

            if (price < MinPrice || price > MaxPrice)
            {
                errorMessage = $"Price must be between {MinPrice:N0} and {MaxPrice:N0} VND.";
                return false;
            }

            string stockText = txtStockQuantity.Text.Trim();
            if (string.IsNullOrWhiteSpace(stockText))
            {
                errorMessage = "Stock quantity cannot be empty.";
                return false;
            }

            if (!int.TryParse(stockText, NumberStyles.Integer, CultureInfo.InvariantCulture, out stockQuantity))
            {
                errorMessage = "Stock quantity must be a valid integer.";
                return false;
            }

            if (stockQuantity < 0)
            {
                errorMessage = "Stock quantity cannot be negative.";
                return false;
            }

            if (stockQuantity > MaxStockQuantity)
            {
                errorMessage = $"Stock quantity cannot exceed {MaxStockQuantity}.";
                return false;
            }

            string? imageUrl = string.IsNullOrWhiteSpace(txtImageUrl.Text) ? null : txtImageUrl.Text.Trim();
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                errorMessage = "Image URL cannot be empty.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!TryValidateInputs(out string validationError, out decimal price, out int stockQuantity))
                {
                    MessageBox.Show(validationError, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Product product = new Product
                {
                    ProductName = txtProductName.Text.Trim(),
                    Model = txtModel.Text.Trim(),
                    Color = txtColor.Text.Trim(),
                    StorageCapacity = txtStorage.Text.Trim(),
                    Price = price,
                    StockQuantity = stockQuantity,
                    UrlImages = string.IsNullOrWhiteSpace(txtImageUrl.Text) ? null : txtImageUrl.Text.Trim(),
                    Status = chkStatus.IsChecked == true,
                    StaffId = _staffId
                };

                if (_editingProduct == null)
                {
                    _productService.CreateProduct(product);
                    MessageBox.Show("Product added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    product.ProductId = _editingProduct.ProductId;

                    product.StaffId = _editingProduct.StaffId;

                    _productService.UpdateProduct(product);
                    MessageBox.Show("Product updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Operation Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}