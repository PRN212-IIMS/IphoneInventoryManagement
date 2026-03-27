using System;
using System.Linq;
using System.Windows;
using Services.Implementations;
using Services.Interfaces;
using WPFApp.ViewModels;

namespace WPFApp.Views.Staff
{
    public partial class OrderDetailWindow : Window
    {
        private readonly IOrderService _orderService;
        private readonly int _orderId;

        public OrderDetailWindow(int orderId)
        {
            InitializeComponent();
            _orderService = new OrderService();
            _orderId = orderId;
            LoadOrderDetail();
        }

        private void LoadOrderDetail()
        {
            try
            {
                var order = _orderService.GetOrderById(_orderId);
                if (order == null)
                {
                    MessageBox.Show("Không tìm thấy đơn hàng.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                txtOrderId.Text = order.OrderId.ToString();
                txtOrderDate.Text = order.OrderDate.ToString("dd/MM/yyyy HH:mm");
                txtCustomer.Text = order.Customer?.FullName ?? "N/A";
                txtStaff.Text = order.Staff?.FullName ?? "N/A";
                txtReceiverName.Text = order.ReceiverName ?? "N/A";
                txtReceiverPhone.Text = order.ReceiverPhone ?? "N/A";
                txtStatus.Text = order.Status ?? "N/A";
                txtShippingAddress.Text = order.ShippingAddress ?? "N/A";

                var items = order.OrderDetails
                    .Select(d => new OrderDetailDisplayItem
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product?.ProductName ?? "N/A",
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice,
                        LineTotal = d.LineTotal
                    })
                    .ToList();

                dgDetails.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load Detail Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}