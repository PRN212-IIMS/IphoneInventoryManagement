using System;
using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services.Implementations;
using Services.Interfaces;

namespace WPFApp.Views.Staff
{
    public partial class StockInListView : UserControl
    {
        private readonly IStockInService _stockInService;
        private readonly int _staffId;

        public StockInListView(int staffId)
        {
            InitializeComponent();
            _stockInService = new StockInService();
            _staffId = staffId;
            LoadStockIns();
        }

        private void LoadStockIns()
        {
            dgStockIns.ItemsSource = null;
            dgStockIns.ItemsSource = _stockInService.GetAllStockIns();
            dgStockIns.Items.Refresh();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                dgStockIns.ItemsSource = _stockInService.SearchStockIns(keyword);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            LoadStockIns();
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateStockInWindow(_staffId)
            {
                Owner = Window.GetWindow(this)
            };

            bool? result = window.ShowDialog();
            if (result == true)
            {
                LoadStockIns();
            }
        }

        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is StockIn stockIn)
            {
                var window = new StockInDetailWindow(stockIn.StockInId)
                {
                    Owner = Window.GetWindow(this)
                };

                window.ShowDialog();
            }
        }
    }
}