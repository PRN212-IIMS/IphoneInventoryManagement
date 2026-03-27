using Services.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Services.Implementations;
using Services.Interfaces;
using WPFApp.Views.Shared;

namespace WPFApp.Views.Customer
{
    public partial class CustomerShellView : UserControl
    {
        private readonly AuthenticatedUser _user;
        private readonly Action _showLogin;
        private readonly int _customerId;
        private readonly IProfileService _profileService = new ProfileService();

        public CustomerShellView(AuthenticatedUser user, Action showLogin)
        {
            InitializeComponent();
            _user = user;
            _showLogin = showLogin;

            _customerId = user.UserId;

            txtAccountName.Text = string.IsNullOrWhiteSpace(user.Email) ? "Customer" : user.Email;
            txtAccountRole.Text = user.Role;

            ShowProducts();
        }

        private void ResetNavButtonStyles()
        {
            var transparentBrush = new SolidColorBrush(Colors.Transparent);

            btnBrowseProducts.Background = transparentBrush;
            btnCreateOrder.Background = transparentBrush;
            btnMyOrders.Background = transparentBrush;
        }

        private void BrowseProducts_Click(object sender, RoutedEventArgs e)
        {
            ShowProducts();
        }

        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            ShowCheckout();
        }

        private void MyOrders_Click(object sender, RoutedEventArgs e)
        {
            ShowOrders();
        }

        private void AccountBar_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AccountPopup.IsOpen = !AccountPopup.IsOpen;
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            AccountPopup.IsOpen = false;
            ShowProfile();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            AccountPopup.IsOpen = false;
            _showLogin?.Invoke();
        }

        private void ShowProducts()
        {
            ResetNavButtonStyles();
            btnBrowseProducts.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#12306F"));

            txtBreadcrumb.Text = "SYSTEM / CUSTOMER WORKSPACE";
            txtTitle.Text = "Browse Products";
            txtSubtitle.Text = "View available products, search by keyword, and add items to cart.";

            ContentHost.Content = new CustomerProductView(_customerId, OpenCheckoutFromProducts);
        }

        private void ShowCheckout()
        {
            ResetNavButtonStyles();
            btnCreateOrder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#12306F"));

            txtBreadcrumb.Text = "SYSTEM / CUSTOMER WORKSPACE";
            txtTitle.Text = "Create Order";
            txtSubtitle.Text = "Review your cart, enter shipping information, and place your order.";

            ContentHost.Content = new CreateOrderView(_customerId, OpenOrdersAfterCheckout);
        }

        private void ShowOrders()
        {
            ResetNavButtonStyles();
            btnMyOrders.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#12306F"));

            txtBreadcrumb.Text = "SYSTEM / CUSTOMER WORKSPACE";
            txtTitle.Text = "Order List";
            txtSubtitle.Text = "Track your orders, view details, and cancel pending orders.";

            ContentHost.Content = new OrderHistoryView(_customerId);
        }

        private void ShowProfile()
        {
            txtBreadcrumb.Text = "SYSTEM / CUSTOMER WORKSPACE";
            txtTitle.Text = "My Profile";
            txtSubtitle.Text = "View and manage your customer information.";

            var roleBadgeBackground = UiFactory.Brush("#E8F2FF");
            var roleBadgeForeground = UiFactory.Brush("#1D4ED8");

            ContentHost.Content = ProfileViewFactory.CreateProfileCard(
                _user,
                ShowEditProfile,
                ShowChangePassword,
                roleBadgeBackground,
                roleBadgeForeground);
        }

        private void ShowEditProfile()
        {
            txtBreadcrumb.Text = "SYSTEM / CUSTOMER WORKSPACE";
            txtTitle.Text = "Edit Profile";
            txtSubtitle.Text = "Update your personal information.";

            ContentHost.Content = ProfileViewFactory.CreateEditProfileCard(
                _user,
                ShowProfile,
                SaveProfileChanges,
                null,
                false);
        }

        private void ShowChangePassword()
        {
            txtBreadcrumb.Text = "SYSTEM / CUSTOMER WORKSPACE";
            txtTitle.Text = "Change Password";
            txtSubtitle.Text = "Update your account password securely.";

            ContentHost.Content = ProfileViewFactory.CreateChangePasswordCard(
                ShowProfile,
                SavePasswordChanges,
                null,
                false);
        }

        private void SaveProfileChanges(string fullName, string phone)
        {
            var result = _profileService.UpdateProfile(new Services.Models.UpdateProfileRequest
            {
                UserId = _user.UserId,
                Role = _user.Role,
                FullName = fullName,
                Phone = phone
            });

            if (!result.Success)
            {
                MessageBox.Show(result.Message);
                return;
            }

            if (result.UpdatedUser is not null)
            {
                _user.FullName = result.UpdatedUser.FullName;
                _user.Phone = result.UpdatedUser.Phone;
                _user.Email = result.UpdatedUser.Email;
                _user.Status = result.UpdatedUser.Status;
                txtAccountName.Text = string.IsNullOrWhiteSpace(_user.Email) ? "Customer" : _user.Email;
            }

            MessageBox.Show(result.Message);
            ShowProfile();
        }

        private void SavePasswordChanges(string currentPassword, string newPassword, string confirmPassword)
        {
            var result = _profileService.ChangePassword(new Services.Models.UpdatePasswordRequest
            {
                UserId = _user.UserId,
                Role = _user.Role,
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmNewPassword = confirmPassword
            });

            MessageBox.Show(result.Message);
            if (result.Success)
            {
                ShowProfile();
            }
        }

        private void OpenCheckoutFromProducts()
        {
            ShowCheckout();
        }

        private void OpenOrdersAfterCheckout()
        {
            ShowOrders();
        }
    }
}