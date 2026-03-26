using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using Services.Models;
using WPFApp.Views.Shared;

namespace WPFApp.Views.Admin;

public partial class AdminShellView : UserControl
{
    private readonly Action _logout;

    public AdminShellView(AuthenticatedUser user, Action logout)
    {
        InitializeComponent();
        _logout = logout;
        BuildLayout(user);
    }

    private void BuildLayout(AuthenticatedUser user)
    {
        var displayName = "System Admin";
        var displayRole = "Admin";

        var root = new Grid { Background = UiFactory.Brush("#F3F7FC") };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(260) });
        root.ColumnDefinitions.Add(new ColumnDefinition());

        var sidebar = BuildSidebar("Admin", new[]
        {
            ("\uE716", "Account List", true),
            ("\uE710", "Create Account", false),
            ("\uE8EF", "Product List", false),
            ("\uE719", "Create Product", false),
        }, displayName, displayRole, "S", false);
        root.Children.Add(sidebar);

        var content = BuildContent("SYSTEM / ACCOUNT LIST", "Account Management", "This layout is ready for feature teams to plug account management content into the main workspace.");
        Grid.SetColumn(content, 1);
        root.Children.Add(content);

        RootHost.Children.Add(root);
    }

    private Grid BuildSidebar(string roleLabel, (string Icon, string Text, bool Active)[] menuItems, string userName, string userRole, string avatar, bool includeProfile)
    {
        var sidebar = new Grid { Background = UiFactory.Brush("#111B31") };
        sidebar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        sidebar.RowDefinitions.Add(new RowDefinition());
        sidebar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var brand = new StackPanel { Margin = new Thickness(22, 26, 22, 0) };
        var brandRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        brandRow.Children.Add(UiFactory.IconBadge("\uE8EA", 38));
        brandRow.Children.Add(new TextBlock { VerticalAlignment = VerticalAlignment.Center, FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 20, Foreground = Brushes.White, Inlines = { new Run("IIMS "), new Run(roleLabel) { Foreground = UiFactory.Brush("#14BEE8") } } });
        brand.Children.Add(brandRow);
        brand.Children.Add(new TextBlock { Text = "INVENTORY MANAGEMENT", FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 12, Foreground = UiFactory.Brush("#5A749C") });
        sidebar.Children.Add(brand);

        var menuStack = new StackPanel { Margin = new Thickness(14, 36, 14, 0), VerticalAlignment = VerticalAlignment.Top };
        foreach (var item in menuItems)
        {
            menuStack.Children.Add(UiFactory.CreateSidebarMenuButton(item.Icon, item.Text, item.Active));
        }
        Grid.SetRow(menuStack, 1);
        sidebar.Children.Add(menuStack);

        var accountHost = new Grid { Margin = new Thickness(14, 0, 14, 18) };
        var accountButton = UiFactory.CreateAccountButton(userName, userRole, avatar);
        var popup = CreateAccountPopup(includeProfile);
        popup.PlacementTarget = accountButton;
        popup.Placement = PlacementMode.Top;
        accountButton.Click += (_, _) => popup.IsOpen = !popup.IsOpen;
        accountHost.Children.Add(accountButton);
        accountHost.Children.Add(popup);
        Grid.SetRow(accountHost, 2);
        sidebar.Children.Add(accountHost);

        return sidebar;
    }

    private Grid BuildContent(string breadcrumb, string title, string subtitle)
    {
        var content = new Grid();
        content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(70) });
        content.RowDefinitions.Add(new RowDefinition());

        var topBar = new Border { Background = Brushes.White, BorderBrush = UiFactory.Brush("#E2EAF3"), BorderThickness = new Thickness(0, 0, 0, 1) };
        topBar.Child = new TextBlock { Margin = new Thickness(28, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 18, Foreground = UiFactory.Brush("#8B9FC0"), Text = breadcrumb };
        content.Children.Add(topBar);

        var body = new StackPanel { Margin = new Thickness(30, 26, 30, 26) };
        body.Children.Add(new TextBlock { Text = title, FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 36, Foreground = UiFactory.Brush("#19345C") });
        body.Children.Add(new TextBlock { Margin = new Thickness(0, 12, 0, 0), Text = subtitle, FontFamily = UiFactory.Font("Bahnschrift"), FontSize = 18, Foreground = UiFactory.Brush("#748CAF") });
        body.Children.Add(new Border { Margin = new Thickness(0, 30, 0, 0), Height = 460, Background = Brushes.White, CornerRadius = new CornerRadius(18), BorderBrush = UiFactory.Brush("#DFE8F2"), BorderThickness = new Thickness(1), Child = new TextBlock { Text = "Workspace ready for module implementation", FontFamily = UiFactory.Font("Bahnschrift"), FontSize = 26, Foreground = UiFactory.Brush("#A5B5CB"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center } });
        Grid.SetRow(body, 1);
        content.Children.Add(body);

        return content;
    }

    private Popup CreateAccountPopup(bool includeProfile)
    {
        var popup = new Popup { AllowsTransparency = true, StaysOpen = false };
        var border = new Border { Width = 230, Padding = new Thickness(18), Background = Brushes.White, CornerRadius = new CornerRadius(14), BorderBrush = UiFactory.Brush("#E5ECF5"), BorderThickness = new Thickness(1) };
        var stack = new StackPanel();
        if (includeProfile)
        {
            stack.Children.Add(CreatePopupAction("\uE77B", "Your Profile", UiFactory.Brush("#445D84"), UiFactory.Brush("#7D95B5"), (_, _) => MessageBox.Show("Profile screen is not connected yet.")));
            stack.Children.Add(new Border { Height = 1, Background = UiFactory.Brush("#EEF2F7"), Margin = new Thickness(0, 8, 0, 8) });
        }
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


