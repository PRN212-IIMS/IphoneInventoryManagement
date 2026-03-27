using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services.Implementations;
using Services.Interfaces;

namespace WPFApp.Views.Staff
{
    public partial class ProductListView : UserControl
    {
        private readonly IProductService _productService;
        private readonly int _staffId;

        public ProductListView(int staffId)
        {
            InitializeComponent();
            _productService = new ProductService();
            _staffId = staffId;

            LoadStatusFilter();
            LoadColorFilter();
            LoadStorageFilter();
            LoadProducts();
        }

        private void LoadProducts()
        {
            dgProducts.ItemsSource = null;
            dgProducts.ItemsSource = _productService.GetAllProducts();
        }

        private void LoadColorFilter()
        {
            var colors = _productService.GetAllColors();
            colors.Insert(0, "All Colors");
            cbColor.ItemsSource = colors;
            cbColor.SelectedIndex = 0;
        }

        private void LoadStorageFilter()
        {
            var storages = _productService.GetAllStorageCapacities();
            storages.Insert(0, "All Storage");
            cbStorage.ItemsSource = storages;
            cbStorage.SelectedIndex = 0;
        }

        private void LoadStatusFilter()
        {
            cbStatus.ItemsSource = new List<string> { "All Status", "Active", "Inactive" };
            cbStatus.SelectedIndex = 0;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string? color = cbColor.SelectedItem?.ToString();
                string? storage = cbStorage.SelectedItem?.ToString();
                string? statusText = cbStatus.SelectedItem?.ToString();

                bool? status = null;
                if (statusText == "Active")
                    status = true;
                else if (statusText == "Inactive")
                    status = false;

                color = color == "All Colors" ? null : color;
                storage = storage == "All Storage" ? null : storage;

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    dgProducts.ItemsSource = _productService.SearchProducts(keyword);
                }
                else
                {
                    dgProducts.ItemsSource = _productService.FilterProducts(color, storage, status);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cbColor.SelectedIndex = 0;
            cbStorage.SelectedIndex = 0;
            cbStatus.SelectedIndex = 0;
            LoadProducts();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var window = new ProductEditWindow(_staffId)
            {
                Owner = Window.GetWindow(this)
            };

            bool? result = window.ShowDialog();
            if (result == true)
            {
                LoadProducts();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is Product product)
            {
                var window = new ProductEditWindow(product, _staffId)
                {
                    Owner = Window.GetWindow(this)
                };

                bool? result = window.ShowDialog();
                if (result == true)
                {
                    LoadProducts();
                }
            }
        }

    }
}