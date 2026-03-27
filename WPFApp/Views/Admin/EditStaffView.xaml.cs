using BusinessObjects;
using BusinessObjects;
using Services.Implementations;
using Services.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace WPFApp.Views.Admin
{
    public partial class EditStaffView : UserControl
    {
        private readonly IStaffService _staffService;
        private readonly int _staffId;
        private readonly Action? _onSaved;
        private readonly Action? _onCancel;

        public EditStaffView(int staffId, Action? onSaved = null, Action? onCancel = null)
        {
            InitializeComponent();
            _staffService = new StaffService();
            _staffId = staffId;
            _onSaved = onSaved;
            _onCancel = onCancel;

            LoadStaffData();
        }

        private void LoadStaffData()
        {
            var staff = _staffService.GetStaffById(_staffId);

            if (staff == null)
            {
                MessageBox.Show("Staff not found.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                _onCancel?.Invoke();
                return;
            }

            txtFullName.Text = staff.FullName;
            txtEmail.Text = staff.Email;
            txtPhone.Text = staff.Phone ?? string.Empty;

            if (string.Equals(staff.Status, "Inactive", StringComparison.OrdinalIgnoreCase))
            {
                cbStatus.SelectedIndex = 1;
            }
            else
            {
                cbStatus.SelectedIndex = 0;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var staff = new BusinessObjects.Staff()
            {
                StaffId = _staffId,
                FullName = txtFullName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Password = txtPassword.Password.Trim(),
                Phone = txtPhone.Text.Trim(),
                Status = GetSelectedStatus()
            }
            ;

            bool result = _staffService.UpdateStaff(staff, out string message);

            MessageBox.Show(
                message,
                result ? "Success" : "Error",
                MessageBoxButton.OK,
                result ? MessageBoxImage.Information : MessageBoxImage.Error);

            if (result)
            {
                _onSaved?.Invoke();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _onCancel?.Invoke();
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