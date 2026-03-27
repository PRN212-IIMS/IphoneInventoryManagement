using Services.Implementations;
using Services.Interfaces;
using Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WPFApp.Views.Admin
{
    public partial class AdminAccountListView : UserControl
    {
        private readonly IAdminAccountService _adminAccountService;

        public AdminAccountListView()
        {
            InitializeComponent();
            _adminAccountService = new AdminAccountService();
            LoadAccounts();
        }

        private void LoadAccounts()
        {
            dgAccounts.ItemsSource = _adminAccountService.GetAllAccounts();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtKeyword.Text.Trim();
            string role = GetSelectedComboBoxValue(cbRole);
            string status = GetSelectedComboBoxValue(cbStatus);

            dgAccounts.ItemsSource = _adminAccountService.SearchAndFilter(keyword, role, status);
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtKeyword.Text = string.Empty;
            cbRole.SelectedIndex = 0;
            cbStatus.SelectedIndex = 0;
            LoadAccounts();
        }

        private void btnChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            if (dgAccounts.SelectedItem is not UserAccount selectedAccount)
            {
                MessageBox.Show("Please select an account first.", "Notification",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedAccount.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("You can only change status for Staff and Customer accounts.",
                    "Notification", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newStatus = selectedAccount.Status.Equals("Active", StringComparison.OrdinalIgnoreCase)
                ? "Inactive"
                : "Active";

            string confirmMessage =
                $"Are you sure you want to change status of '{selectedAccount.FullName}' " +
                $"from '{selectedAccount.Status}' to '{newStatus}'?";

            MessageBoxResult confirmResult = MessageBox.Show(
                confirmMessage,
                "Confirm Status Change",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmResult == MessageBoxResult.No)
            {
                return;
            }

            bool result = _adminAccountService.ChangeAccountStatus(
                selectedAccount.Id,
                selectedAccount.Role,
                newStatus,
                out string message
            );

            MessageBox.Show(message,
                result ? "Success" : "Error",
                MessageBoxButton.OK,
                result ? MessageBoxImage.Information : MessageBoxImage.Error);

            if (result)
            {
                ReloadCurrentData();
            }
        }

        private string GetSelectedComboBoxValue(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem item && item.Content != null)
            {
                return item.Content.ToString() ?? "All";
            }

            return "All";
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not UserAccount selectedAccount)
            {
                MessageBox.Show("Unable to get selected account.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!selectedAccount.Role.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Only Staff accounts can be edited here.", "Notification",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var parentWindow = Window.GetWindow(this);
            if (parentWindow == null)
            {
                MessageBox.Show("Parent window not found.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Window? editWindow = null;

            editWindow = new Window
            {
                Title = "Edit Staff Account",
                Width = 760,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = parentWindow,
                ResizeMode = ResizeMode.NoResize
            };

            editWindow.Content = new EditStaffView(
                selectedAccount.Id,
                onSaved: () =>
                {
                    ReloadCurrentData();
                    editWindow?.Close();
                },
                onCancel: () => editWindow?.Close()
            );

            editWindow.ShowDialog();
        }

        private void ReloadCurrentData()
        {
            string keyword = txtKeyword.Text.Trim();
            string role = GetSelectedComboBoxValue(cbRole);
            string status = GetSelectedComboBoxValue(cbStatus);

            dgAccounts.ItemsSource = _adminAccountService.SearchAndFilter(keyword, role, status);
        }
    }
}
