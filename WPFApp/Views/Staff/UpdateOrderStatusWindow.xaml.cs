using System.Collections.Generic;
using System.Windows;

namespace WPFApp.Views.Staff
{
    public partial class UpdateOrderStatusWindow : Window
    {
        public string SelectedStatus { get; private set; } = string.Empty;

        public UpdateOrderStatusWindow(string currentStatus)
        {
            InitializeComponent();

            cbStatus.ItemsSource = new List<string>
            {
                "Pending",
                "Processing",
                "Completed",
                "Cancelled"
            };

            cbStatus.SelectedItem = currentStatus;
            if (cbStatus.SelectedIndex < 0)
                cbStatus.SelectedIndex = 0;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbStatus.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn trạng thái.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedStatus = cbStatus.SelectedItem.ToString()!;
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}