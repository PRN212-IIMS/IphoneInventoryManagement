using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
        private const int MaxLineQuantity = 1000;
        private const decimal MinImportPrice = 100000m;
        private const decimal MaxImportPrice = 100000000m;
        private const int MaxNoteLength = 500;

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
                    MessageBox.Show("Please select a product.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtQuantity.Text.Trim(), out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("Quantity must be an integer greater than 0.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (quantity > MaxLineQuantity)
                {
                    MessageBox.Show($"Each line cannot exceed {MaxLineQuantity} units.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string importRaw = txtImportPrice.Text.Trim();
                if (string.IsNullOrEmpty(importRaw))
                {
                    MessageBox.Show("Please enter unit import price.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!Regex.IsMatch(importRaw, @"^\d+$"))
                {
                    MessageBox.Show("Unit import price may only contain digits (0-9), with no commas, dots, or other characters.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(importRaw, NumberStyles.None, CultureInfo.InvariantCulture, out decimal importPrice))
                {
                    MessageBox.Show("Unit import price must be a valid number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (importPrice < MinImportPrice || importPrice > MaxImportPrice)
                {
                    MessageBox.Show($"Unit import price must be between {MinImportPrice:N0} and {MaxImportPrice:N0} VND.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (decimal.Truncate(importPrice) != importPrice)
                {
                    MessageBox.Show("Unit import price must be a whole number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var existing = _detailItems.FirstOrDefault(x => x.ProductId == selectedProduct.ProductId);
                if (existing != null)
                {
                    if (existing.Quantity + quantity > MaxLineQuantity)
                    {
                        MessageBox.Show($"Total quantity per product cannot exceed {MaxLineQuantity}.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
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
                    MessageBox.Show("Stock-in must have at least one product line.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string note = txtNote.Text?.Trim() ?? string.Empty;
                if (note.Length > MaxNoteLength)
                {
                    MessageBox.Show($"Note cannot exceed {MaxNoteLength} characters.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var stockIn = new StockIn
                {
                    CreatedByStaffId = _staffId,
                    StockInDate = DateTime.Now,
                    Note = string.IsNullOrWhiteSpace(note) ? null : note
                };

                var details = _detailItems.Select(x => new StockInDetail
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    ImportPrice = x.ImportPrice
                }).ToList();

                _stockInService.CreateStockIn(stockIn, details);

                MessageBox.Show("Stock-in created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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