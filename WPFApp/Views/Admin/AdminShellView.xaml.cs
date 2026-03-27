using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using BusinessObjects;
using Services.Implementations;
using Services.Interfaces;
using Services.Models;
using WPFApp.Views.Shared;

namespace WPFApp.Views.Admin;

public partial class AdminShellView : UserControl
{
    private readonly Action _logout;
    private readonly IStaffService _staffService = new StaffService();
    private readonly IProductService _productService = new ProductService();
    private ContentControl _contentHost = null!;
    private Button _menuAccounts = null!;
    private Button _menuProducts = null!;
    private TextBlock _breadcrumb = null!;
    private TextBlock _title = null!;
    private TextBlock _subtitle = null!;

    public AdminShellView(AuthenticatedUser user, Action logout)
    {
        InitializeComponent();
        _logout = logout;
        BuildLayout(user);
    }

    private void BuildLayout(AuthenticatedUser user)
    {
        var displayName = string.IsNullOrWhiteSpace(user.FullName) ? "System Admin" : user.FullName;
        var displayRole = "Admin";

        var root = new Grid { Background = UiFactory.Brush("#F3F7FC") };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(260) });
        root.ColumnDefinitions.Add(new ColumnDefinition());

        var sidebar = BuildSidebar("Admin", new[]
        {
            ("\uE716", "Staff Accounts", true),
            ("\uE8EF", "Product List", false),
        }, displayName, displayRole, "S", false);
        root.Children.Add(sidebar);

        var content = BuildContent();
        Grid.SetColumn(content, 1);
        root.Children.Add(content);

        RootHost.Children.Add(root);
        ShowStaffAccountsScreen();
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
        _menuAccounts = UiFactory.CreateSidebarMenuButton(menuItems[0].Icon, menuItems[0].Text, true);
        _menuProducts = UiFactory.CreateSidebarMenuButton(menuItems[1].Icon, menuItems[1].Text, false);
        _menuAccounts.Click += (_, _) => ShowStaffAccountsScreen();
        _menuProducts.Click += (_, _) => ShowProductsScreen();
        menuStack.Children.Add(_menuAccounts);
        menuStack.Children.Add(_menuProducts);
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

    private Grid BuildContent()
    {
        var content = new Grid();
        content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) });
        content.RowDefinitions.Add(new RowDefinition());

        var topPanel = new StackPanel { Margin = new Thickness(28, 16, 28, 8) };
        _breadcrumb = new TextBlock { FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 15, Foreground = UiFactory.Brush("#8B9FC0"), Text = "SYSTEM / ADMIN WORKSPACE" };
        _title = new TextBlock { Margin = new Thickness(0, 6, 0, 0), FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 30, Foreground = UiFactory.Brush("#19345C"), Text = "Staff Accounts" };
        _subtitle = new TextBlock { Margin = new Thickness(0, 6, 0, 0), FontFamily = UiFactory.Font("Bahnschrift"), FontSize = 15, Foreground = UiFactory.Brush("#748CAF"), Text = "Create and manage staff accounts." };
        topPanel.Children.Add(_breadcrumb);
        topPanel.Children.Add(_title);
        topPanel.Children.Add(_subtitle);
        content.Children.Add(topPanel);

        _contentHost = new ContentControl { Margin = new Thickness(28, 8, 28, 24) };
        Grid.SetRow(_contentHost, 1);
        content.Children.Add(_contentHost);

        return content;
    }

    private void ShowStaffAccountsScreen()
    {
        SetMenuState(_menuAccounts);
        _title.Text = "Staff Accounts";
        _subtitle.Text = "Admin can create staff account and update active status.";

        var host = new Grid();
        host.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        host.RowDefinitions.Add(new RowDefinition());

        var actionRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        var createButton = UiFactory.CreateButton("Create Staff Account", UiFactory.Brush("#2563EB"), Brushes.White, 34);
        createButton.Width = 170;
        actionRow.Children.Add(createButton);
        host.Children.Add(actionRow);

        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            SelectionMode = DataGridSelectionMode.Single
        };
        grid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("StaffId"), Width = 70 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Full Name", Binding = new System.Windows.Data.Binding("FullName"), Width = 200 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = new System.Windows.Data.Binding("Email"), Width = 220 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Phone", Binding = new System.Windows.Data.Binding("Phone"), Width = 130 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Status", Binding = new System.Windows.Data.Binding("Status"), Width = 100 });
        Grid.SetRow(grid, 1);
        host.Children.Add(grid);

        void LoadStaff() => grid.ItemsSource = _staffService.GetAllStaff();
        LoadStaff();

        createButton.Click += (_, _) => ShowCreateStaffDialog(LoadStaff);
        grid.MouseDoubleClick += (_, _) =>
        {
            if (grid.SelectedItem is BusinessObjects.Staff selected)
            {
                var newStatus = selected.Status == "Active" ? "Inactive" : "Active";
                try
                {
                    _staffService.ChangeStaffStatus(selected.StaffId, newStatus);
                    LoadStaff();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        };

        _contentHost.Content = host;
    }

    private void ShowProductsScreen()
    {
        SetMenuState(_menuProducts);
        _title.Text = "Product List";
        _subtitle.Text = "Admin can view products.";

        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            HeadersVisibility = DataGridHeadersVisibility.Column
        };
        grid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("ProductId"), Width = 70 });
        var imageColumn = new DataGridTemplateColumn { Header = "Image", Width = 90 };
        var imageBorderFactory = new FrameworkElementFactory(typeof(Border));
        imageBorderFactory.SetValue(Border.WidthProperty, 56.0);
        imageBorderFactory.SetValue(Border.HeightProperty, 56.0);
        imageBorderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
        imageBorderFactory.SetValue(Border.BorderBrushProperty, UiFactory.Brush("#E2E8F0"));
        imageBorderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(1));
        imageBorderFactory.SetValue(Border.BackgroundProperty, UiFactory.Brush("#F8FAFC"));

        var imageFactory = new FrameworkElementFactory(typeof(Image));
        imageFactory.SetValue(Image.StretchProperty, Stretch.UniformToFill);
        imageFactory.SetBinding(Image.SourceProperty, new System.Windows.Data.Binding("UrlImages"));
        imageBorderFactory.AppendChild(imageFactory);
        imageColumn.CellTemplate = new DataTemplate { VisualTree = imageBorderFactory };
        grid.Columns.Add(imageColumn);
        grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("ProductName"), Width = 180 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Model", Binding = new System.Windows.Data.Binding("Model"), Width = 120 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Price", Binding = new System.Windows.Data.Binding("Price"), Width = 110 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Stock", Binding = new System.Windows.Data.Binding("StockQuantity"), Width = 90 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Status", Binding = new System.Windows.Data.Binding("Status"), Width = 90 });
        grid.ItemsSource = _productService.GetAllProducts();
        _contentHost.Content = grid;
    }

    private void ShowCreateStaffDialog(Action onCreated)
    {
        var dialog = new Window
        {
            Title = "Create Staff Account",
            Width = 420,
            Height = 420,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize
        };

        var panel = new StackPanel { Margin = new Thickness(16) };
        var fullNameBox = new TextBox { Height = 34 };
        var emailBox = new TextBox { Height = 34 };
        var phoneBox = new TextBox { Height = 34 };
        var passwordBox = new PasswordBox { Height = 34 };
        panel.Children.Add(CreateFormField("Full Name", fullNameBox));
        panel.Children.Add(CreateFormField("Email", emailBox));
        panel.Children.Add(CreateFormField("Phone", phoneBox));
        panel.Children.Add(CreateFormField("Password", passwordBox));

        var saveButton = UiFactory.CreateButton("Create", UiFactory.Brush("#2563EB"), Brushes.White, 34);
        saveButton.Margin = new Thickness(0, 10, 0, 0);
        saveButton.Click += (_, _) =>
        {
            try
            {
                _staffService.CreateStaff(new BusinessObjects.Staff
                {
                    FullName = fullNameBox.Text.Trim(),
                    Email = emailBox.Text.Trim(),
                    Phone = phoneBox.Text.Trim(),
                    Password = passwordBox.Password,
                    Status = "Active"
                });
                onCreated();
                dialog.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        };
        panel.Children.Add(saveButton);
        dialog.Content = panel;
        dialog.ShowDialog();
    }

    private static FrameworkElement CreateFormField(string label, FrameworkElement input)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
        panel.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 0, 0, 4) });
        panel.Children.Add(input);
        return panel;
    }

    private void SetMenuState(Button activeButton)
    {
        _menuAccounts.Background = Brushes.Transparent;
        _menuProducts.Background = Brushes.Transparent;
        activeButton.Background = UiFactory.Brush("#1F3258");
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


