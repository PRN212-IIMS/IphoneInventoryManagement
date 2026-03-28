using System.Collections.Generic;
using System.Windows;

namespace WPFApp.Views.Staff
{
    public partial class UpdateOrderStatusWindow : Window
    {
        private readonly string _currentStatus;
        public string SelectedStatus { get; private set; } = string.Empty;

        public UpdateOrderStatusWindow(string currentStatus)
        {
            InitializeComponent();
            _currentStatus = (currentStatus ?? string.Empty).Trim();

            cbStatus.ItemsSource = new List<string>
            {
                "Pending",
                "Processing",
                "Completed",
                "Cancelled"
            };

            cbStatus.SelectedItem = _currentStatus;
            if (cbStatus.SelectedIndex < 0)
                cbStatus.SelectedIndex = 0;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbStatus.SelectedItem == null)
            {
                MessageBox.Show("Please select a status.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedStatus = cbStatus.SelectedItem.ToString()!;
            if (string.Equals(_currentStatus, SelectedStatus, System.StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("New status must differ from the current status.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidTransition(_currentStatus, SelectedStatus))
            {
                MessageBox.Show($"Cannot change status from '{_currentStatus}' to '{SelectedStatus}'.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private static bool IsValidTransition(string currentStatus, string newStatus)
        {
            return (string.Equals(currentStatus, "Pending", System.StringComparison.OrdinalIgnoreCase)
                        && (string.Equals(newStatus, "Processing", System.StringComparison.OrdinalIgnoreCase)
                            || string.Equals(newStatus, "Cancelled", System.StringComparison.OrdinalIgnoreCase)))
                    || (string.Equals(currentStatus, "Processing", System.StringComparison.OrdinalIgnoreCase)
                        && (string.Equals(newStatus, "Completed", System.StringComparison.OrdinalIgnoreCase)
                            || string.Equals(newStatus, "Cancelled", System.StringComparison.OrdinalIgnoreCase)));
        }
    }
}