using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using Services.Models;
using WPFApp.Views.Shared;

namespace WPFApp.Views.Auth;

public partial class LoginView : UserControl
{
    private readonly Func<string, string, LoginResult> _authenticate;
    private readonly Func<RegisterCustomerRequest, RegisterCustomerResult> _registerCustomer;
    private readonly Action<AuthenticatedUser> _onAuthenticated;
    private Grid _rightPanel = null!;
    private TextBlock _titleBlock = null!;
    private TextBlock _descriptionBlock = null!;
    private TextBlock _hintBlock = null!;
    private Border _illustration = null!;
    private bool _isRegisterMode;
    private string? _loginInfoMessage;

    public LoginView(
        Func<string, string, LoginResult> authenticate,
        Func<RegisterCustomerRequest, RegisterCustomerResult> registerCustomer,
        Action<AuthenticatedUser> onAuthenticated)
    {
        InitializeComponent();
        _authenticate = authenticate;
        _registerCustomer = registerCustomer;
        _onAuthenticated = onAuthenticated;
        BuildLayout();
        ShowLoginForm();
    }

    private void BuildLayout()
    {
        var root = new Grid();
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.45, GridUnitType.Star) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.55, GridUnitType.Star) });

        root.Children.Add(BuildLeftPanel());

        _rightPanel = new Grid { Background = UiFactory.Brush("#F5F8FC") };
        Grid.SetColumn(_rightPanel, 1);
        root.Children.Add(_rightPanel);

        RootHost.Children.Add(root);
    }

    private UIElement BuildLeftPanel()
    {
        var left = new Grid { Background = UiFactory.Brush("#111B31") };
        var shell = new Grid { Margin = new Thickness(46, 34, 46, 34) };
        shell.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        shell.RowDefinitions.Add(new RowDefinition());
        shell.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var brandWrap = new StackPanel();
        var brand = new StackPanel { Orientation = Orientation.Horizontal };
        brand.Children.Add(UiFactory.IconBadge("\uE8EA", 46));
        brand.Children.Add(new TextBlock { VerticalAlignment = VerticalAlignment.Center, FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 24, Foreground = Brushes.White, Inlines = { new Run("iStock "), new Run("Pro") { Foreground = UiFactory.Brush("#14BEE8") } } });
        brandWrap.Children.Add(brand);
        brandWrap.Children.Add(new TextBlock { Text = "ENTERPRISE EDITION", Margin = new Thickness(62, -4, 0, 0), FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 12, Foreground = UiFactory.Brush("#5A749C") });
        shell.Children.Add(brandWrap);

        var hero = new StackPanel { Margin = new Thickness(0, 90, 0, 0), VerticalAlignment = VerticalAlignment.Top };
        _titleBlock = new TextBlock { FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 46, FontWeight = FontWeights.Bold, Foreground = Brushes.White, TextWrapping = TextWrapping.Wrap, MaxWidth = 430 };
        _descriptionBlock = new TextBlock { Margin = new Thickness(0, 28, 0, 0), FontFamily = UiFactory.Font("Bahnschrift"), FontSize = 18, Foreground = UiFactory.Brush("#9AB5D8"), TextWrapping = TextWrapping.Wrap, LineHeight = 38, MaxWidth = 460 };
        _hintBlock = new TextBlock { Margin = new Thickness(0, 18, 0, 0), FontFamily = UiFactory.Font("Bahnschrift"), FontSize = 15, Foreground = UiFactory.Brush("#67CDEA"), TextWrapping = TextWrapping.Wrap };
        hero.Children.Add(_titleBlock);
        hero.Children.Add(new Border { Width = 76, Height = 5, Margin = new Thickness(0, 28, 0, 0), CornerRadius = new CornerRadius(2.5), Background = UiFactory.Brush("#14BEE8") });
        hero.Children.Add(_descriptionBlock);
        hero.Children.Add(_hintBlock);
        Grid.SetRow(hero, 1);
        shell.Children.Add(hero);

        var footer = new StackPanel { Margin = new Thickness(0, 0, 0, 8) };
        footer.Children.Add(CreateFeatureCard("\uE8EF", "Real-time Inventory", "Track stock levels across multiple locations instantly."));
        footer.Children.Add(CreateFeatureCard("\uE72E", "Secure Transactions", "Enterprise-grade security for all your business data."));
        _illustration = new Border
        {
            Width = 320,
            Height = 220,
            CornerRadius = new CornerRadius(34),
            BorderBrush = UiFactory.Brush("#39445F"),
            BorderThickness = new Thickness(16),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 30, -70, -70),
            Opacity = 0.7,
            Child = UiFactory.Mdl2("\uE8B7", 86, UiFactory.Brush("#46516C"))
        };
        footer.Children.Add(_illustration);
        Grid.SetRow(footer, 2);
        shell.Children.Add(footer);

        left.Children.Add(shell);
        return left;
    }

    private Border CreateFeatureCard(string icon, string title, string description)
    {
        var border = new Border { Margin = new Thickness(0, 0, 0, 14), Background = Brushes.Transparent };
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(new Border
        {
            Width = 52,
            Height = 52,
            CornerRadius = new CornerRadius(14),
            Background = UiFactory.Brush("#1A243B"),
            BorderBrush = UiFactory.Brush("#31405C"),
            BorderThickness = new Thickness(1),
            Child = UiFactory.Mdl2(icon, 18, UiFactory.Brush("#14BEE8"))
        });
        var textStack = new StackPanel { Margin = new Thickness(14, 0, 0, 0) };
        textStack.Children.Add(new TextBlock { Text = title, FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 16, Foreground = Brushes.White });
        textStack.Children.Add(new TextBlock { Text = description, Margin = new Thickness(0, 4, 0, 0), FontFamily = UiFactory.Font("Bahnschrift"), FontSize = 14, Foreground = UiFactory.Brush("#6E84A8"), TextWrapping = TextWrapping.Wrap, MaxWidth = 320 });
        row.Children.Add(textStack);
        border.Child = row;
        return border;
    }

    private void ShowLoginForm()
    {
        _isRegisterMode = false;
        UpdateLeftPanel();
        _rightPanel.Children.Clear();

        var card = CreateCard(520, 0);
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = "Welcome Back", FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 28, Foreground = UiFactory.Brush("#16325C") });
        stack.Children.Add(new TextBlock { Text = "Please enter your credentials to access the system.", Margin = new Thickness(0, 10, 0, 0), FontFamily = UiFactory.Font("Bahnschrift"), FontSize = 15, Foreground = UiFactory.Brush("#6B83A7") });

        var emailBox = CreateInputTextBox();
        stack.Children.Add(UiFactory.CreateField("EMAIL ADDRESS", "\uE715", emailBox));

        var passwordField = CreateTogglePasswordField("PASSWORD", "\uE72E");
        stack.Children.Add(passwordField.Container);

        var message = CreateMessageTextBlock();
        if (!string.IsNullOrWhiteSpace(_loginInfoMessage))
        {
            message.Text = _loginInfoMessage;
            message.Foreground = UiFactory.Brush("#1E9D72");
            _loginInfoMessage = null;
        }
        stack.Children.Add(message);

        var signIn = UiFactory.CreateButton("Sign In", UiFactory.Brush("#1D2840"), Brushes.White, 50);
        signIn.Margin = new Thickness(0, 24, 0, 0);
        signIn.Click += (_, _) =>
        {
            var email = emailBox.Text.Trim();
            var password = passwordField.GetPassword();
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                SetError(message, "Please enter email and password.");
                return;
            }

            var loginResult = _authenticate(email, password);
            if (!loginResult.Success || loginResult.User is null)
            {
                SetError(message, loginResult.Message);
                return;
            }

            message.Text = string.Empty;
            _onAuthenticated(loginResult.User);
        };
        stack.Children.Add(signIn);

        stack.Children.Add(new TextBlock { Text = "NEW TO THE SYSTEM?", Margin = new Thickness(0, 34, 0, 14), HorizontalAlignment = HorizontalAlignment.Center, FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 14, Foreground = UiFactory.Brush("#A2B2C9") });
        var createAccount = UiFactory.CreateOutlineButton("Create Customer Account");
        createAccount.Click += (_, _) => ShowRegisterForm();
        stack.Children.Add(createAccount);

        card.Child = stack;
        _rightPanel.Children.Add(card);
    }

    private void ShowRegisterForm()
    {
        _isRegisterMode = true;
        UpdateLeftPanel();
        _rightPanel.Children.Clear();

        var scroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = Brushes.Transparent
        };

        var host = new Grid { HorizontalAlignment = HorizontalAlignment.Center };
        var card = CreateCard(640, 24);
        card.VerticalAlignment = VerticalAlignment.Center;
        card.Margin = new Thickness(28, 24, 28, 24);

        var stack = new StackPanel();
        stack.Children.Add(new Border
        {
            Height = 6,
            Margin = new Thickness(-42, -40, -42, 24),
            CornerRadius = new CornerRadius(18, 18, 0, 0),
            Background = new LinearGradientBrush(((SolidColorBrush)UiFactory.Brush("#14BEE8")).Color, ((SolidColorBrush)UiFactory.Brush("#2D68FF")).Color, 0)
        });
        stack.Children.Add(new TextBlock { Text = "Create Account", FontFamily = UiFactory.Font("Bahnschrift SemiBold"), FontSize = 28, Foreground = UiFactory.Brush("#16325C") });
        stack.Children.Add(new TextBlock { Text = "Register as a customer to use the system", Margin = new Thickness(0, 10, 0, 0), FontFamily = UiFactory.Font("Bahnschrift"), FontSize = 15, Foreground = UiFactory.Brush("#6B83A7") });

        var fullNameBox = CreateInputTextBox();
        stack.Children.Add(UiFactory.CreateField("FULL NAME", "\uE77B", fullNameBox));

        var infoGrid = new Grid { Margin = new Thickness(0, 0, 0, 0) };
        infoGrid.ColumnDefinitions.Add(new ColumnDefinition());
        infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
        infoGrid.ColumnDefinitions.Add(new ColumnDefinition());
        var emailBox = CreateInputTextBox();
        var emailField = UiFactory.CreateField("EMAIL ADDRESS", "\uE715", emailBox);
        Grid.SetColumn(emailField, 0);
        infoGrid.Children.Add(emailField);
        var phoneStack = new StackPanel();
        var phoneBox = CreateInputTextBox();
        phoneStack.Children.Add(UiFactory.CreateField("PHONE NUMBER", "\uE717", phoneBox));
        phoneStack.Children.Add(CreateRequirementText(new[]
        {
            "Phone number must have exactly 10 digits.",
            "Phone number must start with 0."
        }));
        Grid.SetColumn(phoneStack, 2);
        infoGrid.Children.Add(phoneStack);
        stack.Children.Add(infoGrid);

        var passwordGrid = new Grid();
        passwordGrid.ColumnDefinitions.Add(new ColumnDefinition());
        passwordGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
        passwordGrid.ColumnDefinitions.Add(new ColumnDefinition());

        var passwordStack = new StackPanel();
        var passwordField = CreateTogglePasswordField("PASSWORD", "\uE72E");
        passwordStack.Children.Add(passwordField.Container);
        passwordStack.Children.Add(CreateRequirementText(new[]
        {
            "Password must be at least 8 characters.",
            "Include at least 1 uppercase letter.",
            "Include at least 1 number and 1 special character.",
            "Do not use spaces in the password."
        }));
        Grid.SetColumn(passwordStack, 0);
        passwordGrid.Children.Add(passwordStack);

        var confirmStack = new StackPanel();
        var confirmField = CreateTogglePasswordField("CONFIRM PASSWORD", "\uE73E");
        confirmStack.Children.Add(confirmField.Container);
        Grid.SetColumn(confirmStack, 2);
        passwordGrid.Children.Add(confirmStack);
        stack.Children.Add(passwordGrid);

        var message = CreateMessageTextBlock();
        stack.Children.Add(message);

        var registerButton = UiFactory.CreateButton("Register", UiFactory.Brush("#1D2840"), Brushes.White, 56);
        registerButton.Margin = new Thickness(0, 26, 0, 0);
        registerButton.Click += (_, _) =>
        {
            var result = _registerCustomer(new RegisterCustomerRequest
            {
                FullName = fullNameBox.Text,
                Email = emailBox.Text,
                Phone = phoneBox.Text,
                Password = passwordField.GetPassword(),
                ConfirmPassword = confirmField.GetPassword()
            });

            if (!result.Success)
            {
                SetError(message, result.Message);
                return;
            }

            _loginInfoMessage = result.Message;
            ShowLoginForm();
        };
        stack.Children.Add(registerButton);

        var bottom = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 18, 0, 0) };
        bottom.Children.Add(new TextBlock { Text = "Already have an account? ", FontFamily = UiFactory.Font("Bahnschrift"), FontSize = 15, Foreground = UiFactory.Brush("#60789D") });
        var loginHere = new Button
        {
            Content = "Login here",
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = UiFactory.Brush("#0E9CC2"),
            FontFamily = UiFactory.Font("Bahnschrift SemiBold"),
            FontSize = 15,
            Padding = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        loginHere.Click += (_, _) => ShowLoginForm();
        bottom.Children.Add(loginHere);
        stack.Children.Add(bottom);

        card.Child = stack;
        host.Children.Add(card);
        scroll.Content = host;
        _rightPanel.Children.Add(scroll);
    }

    private void UpdateLeftPanel()
    {
        if (_isRegisterMode)
        {
            _titleBlock.Inlines.Clear();
            _titleBlock.Inlines.Add(new Run("iPhone Inventory"));
            _titleBlock.Inlines.Add(new LineBreak());
            _titleBlock.Inlines.Add(new Run("Management System") { Foreground = UiFactory.Brush("#14BEE8") });
            _descriptionBlock.Text = "Create a customer account to browse products and place orders. Access our real-time inventory and premium support.";
            _hintBlock.Text = string.Empty;
            _illustration.Child = UiFactory.Mdl2("\uE8B7", 86, UiFactory.Brush("#46516C"));
        }
        else
        {
            _titleBlock.Inlines.Clear();
            _titleBlock.Inlines.Add(new Run("iPhone Inventory"));
            _titleBlock.Inlines.Add(new LineBreak());
            _titleBlock.Inlines.Add(new Run("Management System") { Foreground = UiFactory.Brush("#14BEE8") });
            _descriptionBlock.Text = "Sign in to manage products, stock, and orders inside the inventory system.";
            _hintBlock.Text = "Use valid system credentials to continue.";
            _illustration.Child = UiFactory.Mdl2("\uE8B7", 86, UiFactory.Brush("#46516C"));
        }
    }

    private static TogglePasswordField CreateTogglePasswordField(string label, string icon)
    {
        var passwordBox = CreateInputPasswordBox();
        var plainTextBox = CreateInputTextBox();
        plainTextBox.Visibility = Visibility.Collapsed;

        passwordBox.VerticalAlignment = VerticalAlignment.Center;
        plainTextBox.VerticalAlignment = VerticalAlignment.Center;
        passwordBox.Padding = new Thickness(0, 2, 0, 2);
        plainTextBox.Padding = new Thickness(0, 2, 0, 2);
        passwordBox.Margin = new Thickness(0, -1, 0, 0);
        plainTextBox.Margin = new Thickness(0, -1, 0, 0);

        var toggleButton = new Button
        {
            Width = 28,
            Height = 28,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Content = CreateEyeIcon(false)
        };

        passwordBox.PasswordChanged += (_, _) =>
        {
            if (plainTextBox.Visibility != Visibility.Visible)
            {
                plainTextBox.Text = passwordBox.Password;
            }
        };

        plainTextBox.TextChanged += (_, _) =>
        {
            if (plainTextBox.Visibility == Visibility.Visible)
            {
                passwordBox.Password = plainTextBox.Text;
            }
        };

        toggleButton.Click += (_, _) =>
        {
            var isVisible = plainTextBox.Visibility == Visibility.Visible;
            if (isVisible)
            {
                passwordBox.Password = plainTextBox.Text;
                plainTextBox.Visibility = Visibility.Collapsed;
                passwordBox.Visibility = Visibility.Visible;
                toggleButton.Content = CreateEyeIcon(false);
            }
            else
            {
                plainTextBox.Text = passwordBox.Password;
                passwordBox.Visibility = Visibility.Collapsed;
                plainTextBox.Visibility = Visibility.Visible;
                toggleButton.Content = CreateEyeIcon(true);
            }
        };

        var stack = new StackPanel { Margin = new Thickness(0, 24, 0, 0) };
        stack.Children.Add(new TextBlock
        {
            Text = label,
            FontFamily = UiFactory.Font("Bahnschrift SemiBold"),
            FontSize = 15,
            Foreground = UiFactory.Brush("#6D7E9C"),
            Margin = new Thickness(0, 0, 0, 10)
        });

        var border = new Border
        {
            Height = 52,
            Background = UiFactory.Brush("#F9FBFE"),
            BorderBrush = UiFactory.Brush("#D6E0EE"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14, 0, 14, 0)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var leadingIcon = UiFactory.Mdl2(icon, 16, UiFactory.Brush("#8BA2C2"), new Thickness(0, 0, 12, 0));
        leadingIcon.VerticalAlignment = VerticalAlignment.Center;
        grid.Children.Add(leadingIcon);

        Grid.SetColumn(passwordBox, 1);
        Grid.SetColumn(plainTextBox, 1);
        grid.Children.Add(passwordBox);
        grid.Children.Add(plainTextBox);

        Grid.SetColumn(toggleButton, 2);
        grid.Children.Add(toggleButton);

        border.Child = grid;
        stack.Children.Add(border);

        return new TogglePasswordField(stack, passwordBox, plainTextBox);
    }

    private static Grid CreateEyeIcon(bool crossed)
    {
        var grid = new Grid { Width = 18, Height = 18 };
        var eye = UiFactory.Mdl2("\uE890", 16, UiFactory.Brush("#8BA2C2"));
        eye.Margin = new Thickness(0);
        grid.Children.Add(eye);

        if (crossed)
        {
            grid.Children.Add(new Border
            {
                Width = 16,
                Height = 1.8,
                Background = UiFactory.Brush("#8BA2C2"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 1, 0, 0),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(-28)
            });
        }

        return grid;
    }

    private sealed class TogglePasswordField
    {
        public TogglePasswordField(FrameworkElement container, PasswordBox passwordBox, TextBox plainTextBox)
        {
            Container = container;
            PasswordBox = passwordBox;
            PlainTextBox = plainTextBox;
        }

        public FrameworkElement Container { get; }
        private PasswordBox PasswordBox { get; }
        private TextBox PlainTextBox { get; }

        public string GetPassword()
        {
            return PlainTextBox.Visibility == Visibility.Visible ? PlainTextBox.Text : PasswordBox.Password;
        }
    }
    private static Border CreateCard(double width, double topMargin)
    {
        return new Border
        {
            Width = width,
            Padding = new Thickness(42, 40, 42, 40),
            Background = Brushes.White,
            CornerRadius = new CornerRadius(20),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(30, topMargin, 30, 30),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 22,
                ShadowDepth = 0,
                Opacity = 0.12,
                Color = Colors.Black
            }
        };
    }

    private static TextBox CreateInputTextBox()
    {
        return new TextBox
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            FontFamily = UiFactory.Font("Bahnschrift"),
            FontSize = 17,
            Foreground = UiFactory.Brush("#526A8F"),
            VerticalContentAlignment = VerticalAlignment.Center
        };
    }

    private static PasswordBox CreateInputPasswordBox()
    {
        return new PasswordBox
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            FontFamily = UiFactory.Font("Bahnschrift"),
            FontSize = 17,
            Foreground = UiFactory.Brush("#526A8F")
        };
    }

    private static TextBlock CreateMessageTextBlock()
    {
        return new TextBlock
        {
            Margin = new Thickness(0, 18, 0, 0),
            FontFamily = UiFactory.Font("Bahnschrift"),
            FontSize = 14,
            Foreground = UiFactory.Brush("#D9534F"),
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static StackPanel CreateRequirementText(string[] lines)
    {
        var stack = new StackPanel { Margin = new Thickness(6, 8, 0, 0) };
        foreach (var line in lines)
        {
            stack.Children.Add(new TextBlock
            {
                Text = "- " + line,
                FontFamily = UiFactory.Font("Bahnschrift"),
                FontSize = 12.5,
                Foreground = UiFactory.Brush("#7E93B3"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 2)
            });
        }

        return stack;
    }
    private static void SetError(TextBlock textBlock, string message)
    {
        textBlock.Foreground = UiFactory.Brush("#D9534F");
        textBlock.Text = message;
    }
}












