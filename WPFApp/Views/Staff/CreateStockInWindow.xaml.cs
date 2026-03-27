using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services.Implementations;
using Services.Interfaces;
using WPFApp.ViewModels;

namespace WPFApp.Views.Staff
{
    public partial class CreateStockInWindow : Window
    {
        private readonly IStockInService _stockInService;
        private readonly IProductService _productService;
        private readonly int _staffId;

        private readonly List<StockInDetailItemViewModel> _detailItems = new();

        public CreateStockInWindow(int staffId)
        {
            InitializeComponent();
            _stockInService = new StockInService();
            _productService = new ProductService();
            _staffId = staffId;

            LoadProducts();
            RefreshDetailGrid();
        }

        private void LoadProducts()
        {
            cbProducts.ItemsSource = _productService.GetAllProducts()
                .Where(p => p.Status)
                .ToList();

            cbProducts.SelectedIndex = cbProducts.Items.Count > 0 ? 0 : -1;
        }

        private void RefreshDetailGrid()
        {
            dgDetails.ItemsSource = null;
            dgDetails.ItemsSource = _detailItems;
            dgDetails.Items.Refresh();
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbProducts.SelectedItem is not Product selectedProduct)
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtQuantity.Text.Trim(), out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Quantity phải là số nguyên > 0.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal importPrice = 0;
                if (!string.IsNullOrWhiteSpace(txtImportPrice.Text))
                {
                    if (!decimal.TryParse(txtImportPrice.Text.Trim(), out importPrice) || importPrice < 0)
                    {
                        MessageBox.Show("Unit Import Price phải là số hợp lệ >= 0.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                var existing = _detailItems.FirstOrDefault(x => x.ProductId == selectedProduct.ProductId);
                if (existing != null)
                {
                    existing.Quantity += quantity;
                    existing.ImportPrice = importPrice;
                }
                else
                {
                    _detailItems.Add(new StockInDetailItemViewModel
                    {
                        ProductId = selectedProduct.ProductId,
                        ProductName = selectedProduct.ProductName,
                        Quantity = quantity,
                        ImportPrice = importPrice
                    });
                }

                RefreshDetailGrid();

                txtQuantity.Clear();
                txtImportPrice.Clear();
                cbProducts.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Add Item Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is StockInDetailItemViewModel item)
            {
                _detailItems.Remove(item);
                RefreshDetailGrid();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_detailItems.Count == 0)
                {
                    MessageBox.Show("Phiếu nhập phải có ít nhất 1 sản phẩm.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var stockIn = new StockIn
                {
                    CreatedByStaffId = _staffId,
                    StockInDate = DateTime.Now,
                    Note = string.IsNullOrWhiteSpace(txtNote.Text) ? null : txtNote.Text.Trim()
                };

                var details = _detailItems.Select(x => new StockInDetail
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    ImportPrice = x.ImportPrice
                }).ToList();

                _stockInService.CreateStockIn(stockIn, details);

                MessageBox.Show("Tạo phiếu nhập thành công.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}