using System.Windows;
using Services.Implementations;
using Services.Interfaces;
using Services.Models;
using WPFApp.Views.Admin;
using WPFApp.Views.Auth;
using WPFApp.Views.Customer;
using WPFApp.Views.Staff;

namespace WPFApp.Views;

public partial class MainWindow : Window
{
    private readonly IAuthService _authService = new AuthService();

    public MainWindow()
    {
        InitializeComponent();
        ShowLogin();
    }

    private void ShowLogin()
    {
        RootHost.Children.Clear();
        RootHost.Children.Add(new LoginView(
            (email, password) => _authService.Login(email, password),
            request => _authService.RegisterCustomer(request),
            HandleAuthenticated));
    }

    private void HandleAuthenticated(AuthenticatedUser user)
    {
        RootHost.Children.Clear();

        switch (user.Role)
        {
            case "Admin":
                RootHost.Children.Add(new AdminShellView(user, ShowLogin));
                break;
            case "Staff":
                RootHost.Children.Add(new StaffShellView(user, ShowLogin));
                break;
            default:
                RootHost.Children.Add(new CustomerShellView(user, ShowLogin));
                break;
        }
    }
}
