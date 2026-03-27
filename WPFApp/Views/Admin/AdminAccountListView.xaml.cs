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
                string keyword = txtKeyword.Text.Trim();
                string role = GetSelectedComboBoxValue(cbRole);
                string status = GetSelectedComboBoxValue(cbStatus);

                dgAccounts.ItemsSource = _adminAccountService.SearchAndFilter(keyword, role, status);
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
    }
}
