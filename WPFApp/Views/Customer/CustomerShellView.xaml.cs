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

namespace WPFApp.Views.Customer;

public partial class CustomerShellView : UserControl
{
    private readonly Action _logout;
    private readonly IProfileService _profileService = new ProfileService();
    private readonly AuthenticatedUser _user;
    private ContentControl _contentHost = null!;

    public CustomerShellView(AuthenticatedUser user, Action logout)
    {
        InitializeComponent();
        _user = user;
        _logout = logout;
        BuildLayout(user);
    }

    private void BuildLayout(AuthenticatedUser user)
    {
        var root = new Grid { Background = UiFactory.Brush("#F3F7FC") };
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(102) });
        root.RowDefinitions.Add(new RowDefinition());

        var topBar = new Border { Background = Brushes.White, BorderBrush = UiFactory.Brush("#DCE6F2"), BorderThickness = new Thickness(0, 0, 0, 1) };
        var topGrid = new Grid { Margin = new Thickness(32, 0, 32, 0) };
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        topGrid.ColumnDefinitions.Add(new ColumnDefinition());
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var brand = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        brand.Children.Add(UiFactory.IconBadge("\uE8EA", 46));
        brand.Children.Add(new TextBlock { VerticalAlignment = VerticalAlignment.Center, FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 24, Foreground = UiFactory.Brush("#19345C"), Inlines = { new Run("iStock "), new Run("Pro") { Foreground = UiFactory.Brush("#14BEE8") } } });
        topGrid.Children.Add(brand);

        var nav = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(34, 0, 0, 0) };
        nav.Children.Add(CreateTopNavButton("Home", true, (_, _) => ShowHome()));
        nav.Children.Add(CreateTopNavButton("Your Orders", false, (_, _) => MessageBox.Show("Order page is not connected yet.")));
        Grid.SetColumn(nav, 1);
        topGrid.Children.Add(nav);

        var accountButton = UiFactory.CreateCustomerAccountButton(_user.FullName);
        var accountPopup = CreateCustomerPopup(_user);
        accountPopup.PlacementTarget = accountButton;
        accountPopup.Placement = PlacementMode.Bottom;
        accountPopup.VerticalOffset = 10;
        accountButton.Click += (_, _) => accountPopup.IsOpen = !accountPopup.IsOpen;
        var accountGrid = new Grid { VerticalAlignment = VerticalAlignment.Center };
        accountGrid.Children.Add(accountButton);
        accountGrid.Children.Add(accountPopup);
        Grid.SetColumn(accountGrid, 3);
        topGrid.Children.Add(accountGrid);

        topBar.Child = topGrid;
        root.Children.Add(topBar);

        _contentHost = new ContentControl();
        Grid.SetRow(_contentHost, 1);
        root.Children.Add(_contentHost);

        RootHost.Children.Add(root);
        ShowHome();
    }

    private void ShowHome()
    {
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var body = new StackPanel { Margin = new Thickness(34, 34, 34, 28) };
        body.Children.Add(CreateHero());
        var cards = new UniformGrid { Columns = 3, Margin = new Thickness(0, 32, 0, 0) };
        cards.Children.Add(CreateInfoCard("\uE8EA", "Latest Arrivals", "Check out the new iPhone 15 series in all colors.", UiFactory.Brush("#14BEE8"), new Thickness(0, 0, 18, 0)));
        cards.Children.Add(CreateInfoCard("\uEAFD", "Order History", "View and track your previous purchases easily.", UiFactory.Brush("#09B980"), new Thickness(9, 0, 9, 0)));
        cards.Children.Add(CreateInfoCard("\uE72E", "Support Center", "Need help? Our team is available 24/7 for you.", UiFactory.Brush("#3A79F7"), new Thickness(18, 0, 0, 0)));
        body.Children.Add(cards);
        scroll.Content = body;
        _contentHost.Content = scroll;
    }

    private void ShowProfile()
    {
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var host = new Grid { Margin = new Thickness(34, 34, 34, 28) };
        var card = ProfileViewFactory.CreateProfileCard(
            _user,
            () => ShowEditProfile(),
            () => MessageBox.Show("Change password is not connected yet."),
            UiFactory.Brush("#DDF8FF"),
            UiFactory.Brush("#0B7BAA"));
        card.Width = 1120;
        card.MaxWidth = 1120;
        card.HorizontalAlignment = HorizontalAlignment.Center;
        card.VerticalAlignment = VerticalAlignment.Top;
        host.Children.Add(card);

        scroll.Content = host;
        _contentHost.Content = scroll;
    }

    private void ShowEditProfile(string? message = null, bool isError = false)
    {
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var host = new Grid { Margin = new Thickness(34, 34, 34, 28) };
        var card = ProfileViewFactory.CreateEditProfileCard(
            _user,
            ShowProfile,
            SaveProfile,
            message,
            isError);
        card.Width = 1120;
        card.MaxWidth = 1120;
        card.HorizontalAlignment = HorizontalAlignment.Center;
        card.VerticalAlignment = VerticalAlignment.Top;
        host.Children.Add(card);
        scroll.Content = host;
        _contentHost.Content = scroll;
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

    private Border CreateHero()
    {
        var hero = new Border { Height = 320, Background = UiFactory.Brush("#111B31"), CornerRadius = new CornerRadius(24), Padding = new Thickness(48, 40, 48, 40) };
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 52, Foreground = Brushes.White, Inlines = { new Run("Welcome to "), new Run("iStock Pro") { Foreground = UiFactory.Brush("#14BEE8") } } });
        stack.Children.Add(new TextBlock { Margin = new Thickness(0, 24, 180, 0), FontFamily = UiFactory.Font("Bahnschrift"), FontSize = 22, Foreground = UiFactory.Brush("#A5C0E3"), TextWrapping = TextWrapping.Wrap, LineHeight = 42, Text = "Browse our premium collection of iPhones. From the latest iPhone 15 Pro to the classic models, we have everything you need with guaranteed quality." });
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 32, 0, 0) };
        actions.Children.Add(UiFactory.CreateButton("Browse Inventory", UiFactory.Brush("#0F9CC1"), Brushes.White, 58, 190));
        actions.Children.Add(UiFactory.CreateButton("Track Orders", UiFactory.Brush("#30394D"), Brushes.White, 58, 170, new Thickness(18, 0, 0, 0)));
        stack.Children.Add(actions);
        hero.Child = stack;
        return hero;
    }

    private Border CreateInfoCard(string icon, string title, string text, Brush iconBrush, Thickness margin)
    {
        var card = new Border { Margin = margin, Padding = new Thickness(30), Background = Brushes.White, CornerRadius = new CornerRadius(18), BorderBrush = UiFactory.Brush("#DFE8F2"), BorderThickness = new Thickness(1) };
        var stack = new StackPanel();
        stack.Children.Add(new Border { Width = 66, Height = 66, CornerRadius = new CornerRadius(18), Background = UiFactory.Brush("#F4F8FD"), Child = UiFactory.Mdl2(icon, 28, iconBrush) });
        stack.Children.Add(new TextBlock { Text = title, Margin = new Thickness(0, 36, 0, 0), FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 24, Foreground = UiFactory.Brush("#19345C") });
        stack.Children.Add(new TextBlock { Text = text, Margin = new Thickness(0, 18, 0, 0), FontFamily = UiFactory.Font("Bahnschrift"), FontSize = 17, Foreground = UiFactory.Brush("#6681A8"), TextWrapping = TextWrapping.Wrap });
        card.Child = stack;
        return card;
    }

    private Popup CreateCustomerPopup(AuthenticatedUser user)
    {
        var popup = new Popup { AllowsTransparency = true, StaysOpen = false };
        var border = new Border { Width = 300, Padding = new Thickness(18), Background = Brushes.White, CornerRadius = new CornerRadius(18), BorderBrush = UiFactory.Brush("#E5ECF5"), BorderThickness = new Thickness(1) };
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = "SIGNED IN AS", FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 14, Foreground = UiFactory.Brush("#93A6C2") });
        stack.Children.Add(new TextBlock { Text = user.Email, Margin = new Thickness(0, 10, 0, 18), FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 16, Foreground = UiFactory.Brush("#243D64") });
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

    private Button CreateTopNavButton(string text, bool active, RoutedEventHandler onClick)
    {
        var button = new Button { Content = text, Margin = new Thickness(active ? 0 : 12, 0, 0, 0), Padding = new Thickness(22, 14, 22, 14), Background = active ? UiFactory.Brush("#E0F8FF") : Brushes.Transparent, Foreground = active ? UiFactory.Brush("#069ED0") : UiFactory.Brush("#58759D"), FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 17, BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand, Template = UiFactory.RoundedButtonTemplate(12) };
        button.Click += onClick;
        return button;
    }
}

