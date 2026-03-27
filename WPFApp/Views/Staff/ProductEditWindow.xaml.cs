using System;
using System.Windows;
using BusinessObjects;
using Services.Implementations;
using Services.Interfaces;

namespace WPFApp.Views.Staff
{
    public partial class ProductEditWindow : Window
    {
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
            txtPrice.Text = product.Price.ToString();
            txtStockQuantity.Text = product.StockQuantity.ToString();
            txtImageUrl.Text = product.UrlImages;
            chkStatus.IsChecked = product.Status;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!decimal.TryParse(txtPrice.Text.Trim(), out decimal price))
                {
                    MessageBox.Show("Price phải là số hợp lệ.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtStockQuantity.Text.Trim(), out int stockQuantity))
                {
                    MessageBox.Show("Stock Quantity phải là số nguyên hợp lệ.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    MessageBox.Show("Thêm sản phẩm thành công.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    product.ProductId = _editingProduct.ProductId;

                    // Giữ StaffId cũ của sản phẩm
                    product.StaffId = _editingProduct.StaffId;

                    _productService.UpdateProduct(product);
                    MessageBox.Show("Cập nhật sản phẩm thành công.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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