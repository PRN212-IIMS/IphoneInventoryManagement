using System;
using System.Linq;
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

namespace WPFApp.Views.Staff;

public partial class StaffShellView : UserControl
{
    private readonly Action _logout;
    private readonly IProfileService _profileService = new ProfileService();
    private readonly IProductService _productService = new ProductService();
    private readonly IStockInService _stockInService = new StockInService();
    private readonly AuthenticatedUser _user;
    private ContentControl _contentHost = null!;
    private Button _menuStockIn = null!;
    private Button _menuProducts = null!;
    private TextBlock _breadcrumb = null!;
    private TextBlock _title = null!;
    private TextBlock _subtitle = null!;

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
        _menuStockIn = UiFactory.CreateSidebarMenuButton("\uE7C3", "Stock In", true);
        _menuProducts = UiFactory.CreateSidebarMenuButton("\uE8EF", "Product List", false);
        _menuStockIn.Click += (_, _) => ShowStockInScreen();
        _menuProducts.Click += (_, _) => ShowProductsScreen();
        menuStack.Children.Add(_menuStockIn);
        menuStack.Children.Add(_menuProducts);
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
        content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        content.RowDefinitions.Add(new RowDefinition());
        Grid.SetColumn(content, 1);

        var topPanel = new StackPanel { Margin = new Thickness(28, 16, 28, 8) };
        _breadcrumb = new TextBlock { FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 15, Foreground = UiFactory.Brush("#8B9FC0"), Text = "SYSTEM / STAFF WORKSPACE" };
        _title = new TextBlock { Margin = new Thickness(0, 6, 0, 0), FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 30, Foreground = UiFactory.Brush("#19345C"), Text = "Stock In" };
        _subtitle = new TextBlock { Margin = new Thickness(0, 6, 0, 0), FontFamily = UiFactory.Font("Bahnschrift"), FontSize = 15, Foreground = UiFactory.Brush("#748CAF"), Text = "Record imported stock into product inventory." };
        topPanel.Children.Add(_breadcrumb);
        topPanel.Children.Add(_title);
        topPanel.Children.Add(_subtitle);
        content.Children.Add(topPanel);

        _contentHost = new ContentControl { Margin = new Thickness(28, 14, 28, 24) };
        Grid.SetRow(_contentHost, 1);
        content.Children.Add(_contentHost);
        root.Children.Add(content);

        RootHost.Children.Add(root);
        ShowStockInScreen();
    }

    private void ShowProductsScreen()
    {
        SetMenuState(_menuProducts);
        _title.Text = "Product List";
        _subtitle.Text = "View, add, and update products.";

        var host = new Grid();
        host.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        host.RowDefinitions.Add(new RowDefinition());

        var actionRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        var refreshButton = UiFactory.CreateButton("Refresh", UiFactory.Brush("#334155"), Brushes.White, 34);
        refreshButton.Width = 100;
        refreshButton.Margin = new Thickness(0, 0, 10, 0);
        actionRow.Children.Add(refreshButton);

        var addButton = UiFactory.CreateButton("Add Product", UiFactory.Brush("#2563EB"), Brushes.White, 34);
        addButton.Width = 130;
        addButton.Margin = new Thickness(0, 0, 10, 0);
        actionRow.Children.Add(addButton);

        var editButton = UiFactory.CreateButton("Edit Product", UiFactory.Brush("#0F766E"), Brushes.White, 34);
        editButton.Width = 130;
        actionRow.Children.Add(editButton);
        host.Children.Add(actionRow);

        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            SelectionMode = DataGridSelectionMode.Single,
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
        grid.Columns.Add(new DataGridTextColumn { Header = "Color", Binding = new System.Windows.Data.Binding("Color"), Width = 90 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Storage", Binding = new System.Windows.Data.Binding("StorageCapacity"), Width = 90 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Price", Binding = new System.Windows.Data.Binding("Price"), Width = 120 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Stock", Binding = new System.Windows.Data.Binding("StockQuantity"), Width = 90 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Status", Binding = new System.Windows.Data.Binding("Status"), Width = 90 });

        Grid.SetRow(grid, 1);
        host.Children.Add(grid);

        void LoadProducts() => grid.ItemsSource = _productService.GetAllProducts();
        LoadProducts();

        refreshButton.Click += (_, _) => LoadProducts();
        addButton.Click += (_, _) => ShowProductEditor(null, LoadProducts);
        editButton.Click += (_, _) =>
        {
            if (grid.SelectedItem is not Product selected)
            {
                MessageBox.Show("Please select a product to edit.");
                return;
            }

            ShowProductEditor(selected, LoadProducts);
        };
        grid.MouseDoubleClick += (_, _) =>
        {
            if (grid.SelectedItem is Product selected)
            {
                ShowProductEditor(selected, LoadProducts);
            }
        };

        _contentHost.Content = host;
    }

    private void ShowProductEditor(Product? product, Action onSaved)
    {
        var isEdit = product is not null;
        var window = new Window
        {
            Title = isEdit ? "Update Product" : "Add Product",
            Width = 460,
            Height = 700,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize
        };

        var panel = new StackPanel { Margin = new Thickness(18) };
        var nameBox = CreateFormTextBox(product?.ProductName);
        var modelBox = CreateFormTextBox(product?.Model);
        var colorBox = CreateFormTextBox(product?.Color);
        var storageBox = CreateFormTextBox(product?.StorageCapacity);
        var priceBox = CreateFormTextBox(product?.Price.ToString() ?? "0");
        var stockBox = CreateFormTextBox(product?.StockQuantity.ToString() ?? "0");
        var imageBox = CreateFormTextBox(product?.UrlImages);
        var statusBox = new ComboBox { Height = 34 };
        statusBox.Items.Add("Active");
        statusBox.Items.Add("Inactive");
        statusBox.SelectedItem = (product?.Status ?? true) ? "Active" : "Inactive";

        panel.Children.Add(CreateFormField("Name", nameBox));
        panel.Children.Add(CreateFormField("Model", modelBox));
        panel.Children.Add(CreateFormField("Color", colorBox));
        panel.Children.Add(CreateFormField("Storage", storageBox));
        panel.Children.Add(CreateFormField("Price", priceBox));
        panel.Children.Add(CreateFormField("Stock", stockBox));
        panel.Children.Add(CreateFormField("Image URL", imageBox));
        panel.Children.Add(CreateFormField("Status", statusBox));

        var actionRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 14, 0, 0)
        };

        var cancel = UiFactory.CreateButton("Cancel", UiFactory.Brush("#64748B"), Brushes.White, 36);
        cancel.Width = 100;
        cancel.Margin = new Thickness(0, 0, 10, 0);
        cancel.Click += (_, _) => window.Close();

        var save = UiFactory.CreateButton("Save", UiFactory.Brush("#2563EB"), Brushes.White, 36);
        save.Width = 100;
        save.Click += (_, _) =>
        {
            try
            {
                var target = product ?? new Product();
                target.StaffId = _user.UserId;
                target.ProductName = nameBox.Text.Trim();
                target.Model = modelBox.Text.Trim();
                target.Color = colorBox.Text.Trim();
                target.StorageCapacity = storageBox.Text.Trim();
                target.UrlImages = imageBox.Text.Trim();
                target.Status = statusBox.SelectedItem?.ToString() == "Active";
                target.Price = decimal.Parse(priceBox.Text.Trim());
                target.StockQuantity = int.Parse(stockBox.Text.Trim());

                if (isEdit)
                {
                    _productService.UpdateProduct(target);
                }
                else
                {
                    _productService.CreateProduct(target);
                }

                onSaved();
                window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        };

        actionRow.Children.Add(cancel);
        actionRow.Children.Add(save);
        panel.Children.Add(actionRow);

        window.Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = panel
        };
        window.ShowDialog();
    }

    private void ShowStockInScreen()
    {
        SetMenuState(_menuStockIn);
        _title.Text = "Stock In";
        _subtitle.Text = "Record imported stock into product inventory.";

        var host = new Grid();
        host.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        host.RowDefinitions.Add(new RowDefinition());

        var topCard = new Border
        {
            Background = Brushes.White,
            BorderBrush = UiFactory.Brush("#DFE8F2"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(14),
            Margin = new Thickness(0, 0, 0, 12)
        };

        var topGrid = new Grid();
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2.8, GridUnitType.Star) });
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.4, GridUnitType.Star) });
        topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

        var productCombo = new ComboBox { Height = 36, Margin = new Thickness(0, 0, 10, 0) };
        var quantityBox = CreateFormTextBox("1");
        quantityBox.Height = 36;
        quantityBox.Margin = new Thickness(0, 0, 10, 0);
        var importPriceBox = CreateFormTextBox("0");
        importPriceBox.Height = 36;
        importPriceBox.Margin = new Thickness(0, 0, 10, 0);
        var submit = UiFactory.CreateButton("Stock In", UiFactory.Brush("#059669"), Brushes.White, 36);
        submit.Width = 110;
        submit.VerticalAlignment = VerticalAlignment.Bottom;

        var productField = CreateFormField("Product", productCombo);
        var quantityField = CreateFormField("Quantity", quantityBox);
        var importPriceField = CreateFormField("Import Price", importPriceBox);

        Grid.SetColumn(productField, 0);
        Grid.SetColumn(quantityField, 1);
        Grid.SetColumn(importPriceField, 2);
        Grid.SetColumn(submit, 3);

        topGrid.Children.Add(productField);
        topGrid.Children.Add(quantityField);
        topGrid.Children.Add(importPriceField);
        topGrid.Children.Add(submit);
        topCard.Child = topGrid;
        host.Children.Add(topCard);

        var stockGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            RowHeaderWidth = 0
        };
        stockGrid.Columns.Add(new DataGridTextColumn { Header = "StockIn ID", Binding = new System.Windows.Data.Binding("StockInId"), Width = 100 });
        stockGrid.Columns.Add(new DataGridTextColumn { Header = "Date", Binding = new System.Windows.Data.Binding("StockInDate"), Width = 160 });
        stockGrid.Columns.Add(new DataGridTextColumn { Header = "Staff", Binding = new System.Windows.Data.Binding("CreatedByStaff.FullName"), Width = 180 });
        stockGrid.Columns.Add(new DataGridTextColumn { Header = "Items", Binding = new System.Windows.Data.Binding("StockInDetails.Count"), Width = 80 });
        Grid.SetRow(stockGrid, 1);
        host.Children.Add(stockGrid);

        void LoadProductsToCombo()
        {
            var products = _productService.GetAllProducts();
            productCombo.ItemsSource = products;
            productCombo.DisplayMemberPath = "ProductName";
            productCombo.SelectedValuePath = "ProductId";
            productCombo.SelectedIndex = products.Count > 0 ? 0 : -1;
        }

        void LoadStockIns()
        {
            stockGrid.ItemsSource = _stockInService.GetAllStockIns();
        }

        LoadProductsToCombo();
        LoadStockIns();

        submit.Click += (_, _) =>
        {
            try
            {
                if (productCombo.SelectedItem is not Product selectedProduct)
                {
                    MessageBox.Show("Please choose a product.");
                    return;
                }

                var quantity = int.Parse(quantityBox.Text.Trim());
                var importPrice = decimal.Parse(importPriceBox.Text.Trim());
                var stockIn = new StockIn
                {
                    CreatedByStaffId = _user.UserId,
                    Note = null
                };
                var details = new List<StockInDetail>
                {
                    new()
                    {
                        ProductId = selectedProduct.ProductId,
                        Quantity = quantity,
                        ImportPrice = importPrice
                    }
                };

                _stockInService.CreateStockIn(stockIn, details);
                quantityBox.Text = "1";
                importPriceBox.Text = "0";
                LoadStockIns();
                MessageBox.Show("Stock in created successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        };

        _contentHost.Content = host;
    }

    private static TextBox CreateFormTextBox(string? value)
    {
        return new TextBox { Height = 34, Text = value ?? string.Empty };
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
        _menuProducts.Background = Brushes.Transparent;
        _menuStockIn.Background = Brushes.Transparent;
        activeButton.Background = UiFactory.Brush("#1F3258");
    }

    private void ShowProfile()
    {
        var host = new Grid { Margin = new Thickness(26, 22, 26, 22) };
        var card = ProfileViewFactory.CreateProfileCard(
            _user,
            () => ShowEditProfile(),
            () => ShowChangePassword(),
            UiFactory.Brush("#ECF3FF"),
            UiFactory.Brush("#4E73C7"));
        host.Children.Add(card);
        _contentHost.Content = host;
    }

    private void ShowEditProfile(string? message = null, bool isError = false)
    {
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var host = new Grid { Margin = new Thickness(22, 18, 22, 18) };
        var card = ProfileViewFactory.CreateEditProfileCard(
            _user,
            ShowProfile,
            SaveProfile,
            message,
            isError);
        card.Width = 960;
        card.MaxWidth = 960;
        card.HorizontalAlignment = HorizontalAlignment.Center;
        card.VerticalAlignment = VerticalAlignment.Top;
        host.Children.Add(card);
        scroll.Content = host;
        _contentHost.Content = scroll;
    }

    private void ShowChangePassword(string? message = null, bool isError = false, string currentPassword = "", string newPassword = "", string confirmNewPassword = "")
    {
        var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var host = new Grid { Margin = new Thickness(22, 18, 22, 18) };
        var card = ProfileViewFactory.CreateChangePasswordCard(
            ShowProfile,
            SavePassword,
            message,
            isError,
            currentPassword,
            newPassword,
            confirmNewPassword);
        card.Width = 960;
        card.MaxWidth = 960;
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
            _user.FullName = result.UpdatedUser.FullName;
            _user.Phone = result.UpdatedUser.Phone;
            _user.Status = result.UpdatedUser.Status;
            _user.Email = result.UpdatedUser.Email;
        }

        RootHost.Children.Clear();
        BuildLayout(_user);
        ShowProfile();
    }

    private void SavePassword(string currentPassword, string newPassword, string confirmNewPassword)
    {
        var result = _profileService.ChangePassword(new UpdatePasswordRequest
        {
            UserId = _user.UserId,
            Role = _user.Role,
            CurrentPassword = currentPassword,
            NewPassword = newPassword,
            ConfirmNewPassword = confirmNewPassword
        });

        if (!result.Success)
        {
            ShowChangePassword(result.Message, true, currentPassword, newPassword, confirmNewPassword);
            return;
        }

        ShowChangePassword(result.Message, false);
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







