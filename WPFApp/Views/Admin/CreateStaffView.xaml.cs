using System.Windows;
using System.Windows.Controls;
using BusinessObjects;
using Services.Implementations;
using Services.Interfaces;

namespace WPFApp.Views.Admin
{
    public partial class CreateStaffView : UserControl
    {
        private readonly IStaffService _staffService;
        private readonly Action? _onCreated;

        public CreateStaffView(Action? onCreated = null)
        {
            InitializeComponent();
            _staffService = new StaffService();
            _onCreated = onCreated;
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            var staff = new BusinessObjects.Staff
            {
                FullName = txtFullName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Password = txtPassword.Password.Trim(),
                Phone = txtPhone.Text.Trim(),
                Status = GetSelectedStatus()
            };

            bool result = _staffService.CreateStaff(staff, out string message);

            MessageBox.Show(
                message,
                result ? "Success" : "Error",
                MessageBoxButton.OK,
                result ? MessageBoxImage.Information : MessageBoxImage.Error);

            if (result)
            {
                _onCreated?.Invoke();
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void ResetForm()
        {
            txtFullName.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtPassword.Password = string.Empty;
            txtPhone.Text = string.Empty;
            cbStatus.SelectedIndex = 0;
            txtFullName.Focus();
        }

        private string GetSelectedStatus()
        {
            if (cbStatus.SelectedItem is ComboBoxItem item && item.Content != null)
            {
                return item.Content.ToString() ?? "Active";
            }

            return "Active";
        }
    }
}