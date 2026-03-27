using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace WPFApp.Views.Customer
{
    public partial class OrderHistoryView : UserControl
    {
        private readonly int _customerId;

        public OrderHistoryView(int customerId)
        {
            InitializeComponent();
            _customerId = customerId;
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                var serviceType = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType("Services.Implementations.OrderService", false))
                    .FirstOrDefault(t => t != null);

                if (serviceType == null)
                {
                    dgOrders.ItemsSource = null;
                    return;
                }

                var service = Activator.CreateInstance(serviceType!);
                if (service == null)
                {
                    dgOrders.ItemsSource = null;
                    return;
                }

                var methods = serviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                var method = methods.FirstOrDefault(m =>
                    m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType == typeof(int)
                    && (string.Equals(m.Name, "GetOrdersByCustomerId", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(m.Name, "GetOrdersOfCustomer", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(m.Name, "GetCustomerOrders", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(m.Name, "GetOrdersByCustomer", StringComparison.OrdinalIgnoreCase)));

                object? result = null;
                if (method != null)
                {
                    result = method.Invoke(service, new object[] { _customerId });
                }
                else
                {
                    var allMethod = methods.FirstOrDefault(m =>
                        m.GetParameters().Length == 0
                        && (string.Equals(m.Name, "GetOrders", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(m.Name, "GetAllOrders", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(m.Name, "GetAll", StringComparison.OrdinalIgnoreCase)));

                    result = allMethod?.Invoke(service, null);
                }

                if (result is IEnumerable enumerable)
                {
                    var data = enumerable.Cast<object>().ToList();

                    if (method == null)
                    {
                        data = data.Where(x =>
                        {
                            var prop = x.GetType().GetProperty("CustomerId");
                            return prop != null && Equals(prop.GetValue(x), _customerId);
                        }).ToList();
                    }

                    dgOrders.ItemsSource = data;
                    return;
                }

                dgOrders.ItemsSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void ViewDetail_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgOrders.SelectedItem;
            if (selected == null)
            {
                MessageBox.Show("Please select an order.");
                return;
            }

            var orderIdProp = selected.GetType().GetProperty("OrderId");
            if (orderIdProp == null)
            {
                MessageBox.Show("Cannot read selected order.");
                return;
            }

            int orderId = Convert.ToInt32(orderIdProp.GetValue(selected));
            var detailWindow = new OrderDetailWindow(_customerId, orderId);
            bool? result = detailWindow.ShowDialog();
            if (result == true)
            {
                LoadOrders();
            }
        }
    }
}