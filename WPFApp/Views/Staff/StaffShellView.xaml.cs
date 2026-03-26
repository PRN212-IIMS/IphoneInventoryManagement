using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using Services.Implementations;
using Services.Interfaces;
using Services.Models;
using WPFApp.Views.Shared;

namespace WPFApp.Views.Staff;

public partial class StaffShellView : UserControl
{
    private readonly Action _logout;
    private readonly IProfileService _profileService = new ProfileService();
    private readonly AuthenticatedUser _user;
    private ContentControl _contentHost = null!;

    public StaffShellView(AuthenticatedUser user, Action logout)
    {
        InitializeComponent();
        _user = user;
        _logout = logout;
        BuildLayout(user);
    }

    private void BuildLayout(AuthenticatedUser user)
    {
        var root = new Grid { Background = UiFactory.Brush("#F3F7FC") };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(260) });
        root.ColumnDefinitions.Add(new ColumnDefinition());

        var sidebar = new Grid { Background = UiFactory.Brush("#111B31") };
        sidebar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        sidebar.RowDefinitions.Add(new RowDefinition());
        sidebar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var brand = new StackPanel { Margin = new Thickness(22, 26, 22, 0) };
        var brandRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        brandRow.Children.Add(UiFactory.IconBadge("\uE8EA", 38));
        brandRow.Children.Add(new TextBlock { VerticalAlignment = VerticalAlignment.Center, FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 20, Foreground = Brushes.White, Inlines = { new Run("IIMS "), new Run("Staff") { Foreground = UiFactory.Brush("#14BEE8") } } });
        brand.Children.Add(brandRow);
        brand.Children.Add(new TextBlock { Text = "INVENTORY MANAGEMENT", FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 12, Foreground = UiFactory.Brush("#5A749C") });
        sidebar.Children.Add(brand);

        var menuStack = new StackPanel { Margin = new Thickness(14, 36, 14, 0), VerticalAlignment = VerticalAlignment.Top };
        menuStack.Children.Add(UiFactory.CreateSidebarMenuButton("\uE7C3", "Stock Product", false));
        menuStack.Children.Add(UiFactory.CreateSidebarMenuButton("\uE8EF", "Product List", false));
        menuStack.Children.Add(UiFactory.CreateSidebarMenuButton("\uE8D2", "Order List", false));
        Grid.SetRow(menuStack, 1);
        sidebar.Children.Add(menuStack);

        var accountHost = new Grid { Margin = new Thickness(14, 0, 14, 18) };
        var avatar = string.IsNullOrWhiteSpace(user.FullName) ? "S" : user.FullName.Trim()[0].ToString().ToUpperInvariant();
        var accountButton = UiFactory.CreateAccountButton(_user.FullName, _user.Role, avatar);
        var popup = CreateAccountPopup();
        popup.PlacementTarget = accountButton;
        popup.Placement = PlacementMode.Top;
        accountButton.Click += (_, _) => popup.IsOpen = !popup.IsOpen;
        accountHost.Children.Add(accountButton);
        accountHost.Children.Add(popup);
        Grid.SetRow(accountHost, 2);
        sidebar.Children.Add(accountHost);
        root.Children.Add(sidebar);

        var content = new Grid();
        content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(70) });
        content.RowDefinitions.Add(new RowDefinition());
        Grid.SetColumn(content, 1);

        var topBar = new Border { Background = Brushes.White, BorderBrush = UiFactory.Brush("#E2EAF3"), BorderThickness = new Thickness(0, 0, 0, 1) };
        topBar.Child = new TextBlock { Margin = new Thickness(28, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 18, Foreground = UiFactory.Brush("#8B9FC0"), Text = "SYSTEM / STAFF WORKSPACE" };
        content.Children.Add(topBar);

        _contentHost = new ContentControl();
        Grid.SetRow(_contentHost, 1);
        content.Children.Add(_contentHost);
        root.Children.Add(content);

        RootHost.Children.Add(root);
        ShowWorkspace();
    }

    private void ShowWorkspace()
    {
        var body = new StackPanel { Margin = new Thickness(30, 26, 30, 26) };
        body.Children.Add(new TextBlock { Text = "Staff Workspace", FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 36, Foreground = UiFactory.Brush("#19345C") });
        body.Children.Add(new TextBlock { Margin = new Thickness(0, 12, 0, 0), Text = "This workspace is ready for feature teams to plug stock, product, and order modules into the staff area.", FontFamily = UiFactory.Font("Bahnschrift"), FontSize = 18, Foreground = UiFactory.Brush("#748CAF") });
        body.Children.Add(new Border { Margin = new Thickness(0, 30, 0, 0), Height = 460, Background = Brushes.White, CornerRadius = new CornerRadius(18), BorderBrush = UiFactory.Brush("#DFE8F2"), BorderThickness = new Thickness(1), Child = new TextBlock { Text = "Workspace ready for module implementation", FontFamily = UiFactory.Font("Bahnschrift"), FontSize = 26, Foreground = UiFactory.Brush("#A5B5CB"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center } });
        _contentHost.Content = body;
    }

    private void ShowProfile()
    {
        var host = new Grid { Margin = new Thickness(26, 22, 26, 22) };
        var card = ProfileViewFactory.CreateProfileCard(
            _user,
            () => ShowEditProfile(),
            () => MessageBox.Show("Change password is not connected yet."),
            UiFactory.Brush("#ECF3FF"),
            UiFactory.Brush("#4E73C7"));
        host.Children.Add(card);
        _contentHost.Content = host;
    }

    private void ShowEditProfile(string? message = null, bool isError = false)
    {
        var host = new Grid { Margin = new Thickness(26, 22, 26, 22) };
        var card = ProfileViewFactory.CreateEditProfileCard(
            _user,
            ShowProfile,
            SaveProfile,
            message,
            isError);
        host.Children.Add(card);
        _contentHost.Content = host;
    }

    private void SaveProfile(string fullName, string phone)
    {
        var result = _profileService.UpdateProfile(new UpdateProfileRequest
        {
            UserId = _user.UserId,
            Role = _user.Role,
            FullName = fullName,
            Phone = phone
        });

        if (!result.Success)
        {
            ShowEditProfile(result.Message, true);
            return;
        }

        if (result.UpdatedUser is not null)
        {
            ApplyUpdatedUser(result.UpdatedUser);
        }

        ReloadLayout(ShowProfile);
    }

    private void ApplyUpdatedUser(AuthenticatedUser updatedUser)
    {
        _user.FullName = updatedUser.FullName;
        _user.Phone = updatedUser.Phone;
        _user.Status = updatedUser.Status;
        _user.Email = updatedUser.Email;
    }

    private void ReloadLayout(Action nextView)
    {
        RootHost.Children.Clear();
        BuildLayout(_user);
        nextView();
    }

    private Popup CreateAccountPopup()
    {
        var popup = new Popup { AllowsTransparency = true, StaysOpen = false };
        var border = new Border { Width = 230, Padding = new Thickness(18), Background = Brushes.White, CornerRadius = new CornerRadius(14), BorderBrush = UiFactory.Brush("#E5ECF5"), BorderThickness = new Thickness(1) };
        var stack = new StackPanel();
        stack.Children.Add(CreatePopupAction("\uE77B", "Your Profile", UiFactory.Brush("#445D84"), UiFactory.Brush("#7D95B5"), (_, _) =>
        {
            popup.IsOpen = false;
            ShowProfile();
        }));
        stack.Children.Add(new Border { Height = 1, Background = UiFactory.Brush("#EEF2F7"), Margin = new Thickness(0, 8, 0, 8) });
        stack.Children.Add(CreatePopupAction("\uE8AC", "Logout", UiFactory.Brush("#FF4444"), UiFactory.Brush("#FF4444"), (_, _) => _logout()));
        border.Child = stack;
        popup.Child = border;
        return popup;
    }

    private Button CreatePopupAction(string icon, string text, Brush textBrush, Brush iconBrush, RoutedEventHandler onClick)
    {
        var button = new Button { Background = Brushes.Transparent, BorderThickness = new Thickness(0), HorizontalContentAlignment = HorizontalAlignment.Left, Padding = new Thickness(6, 8, 6, 8), Cursor = System.Windows.Input.Cursors.Hand };
        button.Click += onClick;
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(UiFactory.Mdl2(icon, 18, iconBrush, new Thickness(0, 0, 12, 0)));
        row.Children.Add(new TextBlock { Text = text, FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 15, Foreground = textBrush });
        button.Content = row;
        return button;
    }
}

