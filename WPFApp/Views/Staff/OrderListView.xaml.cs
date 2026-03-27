using BusinessObjects;
using Services.Implementations;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Views.Customer;

namespace WPFApp.Views.Staff
{
    public partial class OrderListView : UserControl
    {
        private readonly IOrderService _orderService;
        private readonly int _staffId;

        public OrderListView(int staffId)
        {
            InitializeComponent();
            _orderService = new OrderService();
            _staffId = staffId;

            LoadStatusFilter();
            LoadOrders();
        }

        private void LoadOrders()
        {
            dgOrders.ItemsSource = null;
            dgOrders.ItemsSource = _orderService.GetAllOrders();
            dgOrders.Items.Refresh();
        }

        private void LoadStatusFilter()
        {
            cbStatus.ItemsSource = new List<string>
            {
                "All Status",
                "Pending",
                "Processing",
                "Completed",
                "Cancelled"
            };
            cbStatus.SelectedIndex = 0;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string keyword = txtSearch.Text.Trim();
                string? status = cbStatus.SelectedItem?.ToString();

                status = status == "All Status" ? null : status;

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    dgOrders.ItemsSource = _orderService.SearchOrders(keyword);
                }
                else
                {
                    dgOrders.ItemsSource = _orderService.FilterOrders(status, null, null);
                }

                dgOrders.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Search Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            cbStatus.SelectedIndex = 0;
            LoadOrders();
        }

        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is Order order)
            {
                var window = new OrderDetailWindow(order.OrderId)
                {
                    Owner = Window.GetWindow(this)
                };

                window.ShowDialog();
            }
        }

        private void btnUpdateStatus_Click(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is Order order)
            {
                if (string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(order.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"Order đang ở trạng thái '{order.Status}' nên không thể thay đổi nữa.",
                            "Cannot Update",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    return;
                }

                var window = new UpdateOrderStatusWindow(order.Status ?? "Pending")
                {
                    Owner = Window.GetWindow(this)
                };

                bool? result = window.ShowDialog();
                if (result == true)
                {
                    try
                    {
                        _orderService.UpdateOrderStatus(order.OrderId, window.SelectedStatus);
                        LoadOrders();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Update Failed",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            var window = new CreateOrderWindow(_staffId)
            {
                Owner = Window.GetWindow(this)
            };

            bool? result = window.ShowDialog();
            if (result == true)
            {
                LoadOrders();
            }
        }
    }
}