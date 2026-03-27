using Services.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using WPFApp.Views.Shared;

namespace WPFApp.Views.Admin
{
    public partial class AdminShellView : UserControl
    {
        private readonly Action _logout;
        private ContentControl _mainContentHost = null!;
        private Button? _btnAccountList;
        private Button? _btnCreateAccount;

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
            }, displayName, displayRole, "S", false);
            root.Children.Add(sidebar);

            _mainContentHost = new ContentControl();
            Grid.SetColumn(_mainContentHost, 1);
            root.Children.Add(_mainContentHost);

            ShowAccountList();
            SetActiveMenu("Account List");

            RootHost.Children.Add(root);
        }

        private void ShowAccountList()
        {
            _mainContentHost.Content = new AdminAccountListView();
        }

        private void ShowCreateStaff()
        {
            _mainContentHost.Content = new CreateStaffView(() =>
            {
                ShowAccountList();
                SetActiveMenu("Account List");
            });
        }

        private void SetActiveMenu(string menuName)
        {
            if (_btnAccountList != null)
            {
                _btnAccountList.Background = menuName == "Account List"
                    ? UiFactory.Brush("#14BEE8")
                    : Brushes.Transparent;
            }

            if (_btnCreateAccount != null)
            {
                _btnCreateAccount.Background = menuName == "Create Account"
                    ? UiFactory.Brush("#14BEE8")
                    : Brushes.Transparent;
            }
        }

        private Grid BuildSidebar(string roleLabel, (string Icon, string Text, bool Active)[] menuItems,
            string userName, string userRole, string avatar, bool includeProfile)
        {
            var sidebar = new Grid { Background = UiFactory.Brush("#111B31") };
            sidebar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            sidebar.RowDefinitions.Add(new RowDefinition());
            sidebar.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var brand = new StackPanel { Margin = new Thickness(22, 26, 22, 0) };
            var brandRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            brandRow.Children.Add(UiFactory.IconBadge("\uE8EA", 38));
            brandRow.Children.Add(new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = UiFactory.Font("Bahnschrift SemiBold"),
                FontSize = 20,
                Foreground = Brushes.White,
                Inlines =
                {
                    new Run("IIMS "),
                    new Run(roleLabel) { Foreground = UiFactory.Brush("#14BEE8") }
                }
            });
            brand.Children.Add(brandRow);
            brand.Children.Add(new TextBlock
            {
                Text = "INVENTORY MANAGEMENT",
                FontFamily = UiFactory.Font("Bahnschrift SemiBold"),
                FontSize = 12,
                Foreground = UiFactory.Brush("#5A749C")
            });
            sidebar.Children.Add(brand);

            var menuStack = new StackPanel { Margin = new Thickness(14, 36, 14, 0), VerticalAlignment = VerticalAlignment.Top };

            foreach (var item in menuItems)
            {
                var button = UiFactory.CreateSidebarMenuButton(item.Icon, item.Text, item.Active);

                if (item.Text == "Account List")
                {
                    _btnAccountList = button;
                    button.Click += (_, _) =>
                    {
                        ShowAccountList();
                        SetActiveMenu("Account List");
                    };
                }
                else if (item.Text == "Create Account")
                {
                    _btnCreateAccount = button;
                    button.Click += (_, _) =>
                    {
                        ShowCreateStaff();
                        SetActiveMenu("Create Account");
                    };
                }

                menuStack.Children.Add(button);
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

        private Popup CreateAccountPopup(bool includeProfile)
        {
            var popup = new Popup
            {
                AllowsTransparency = true,
                StaysOpen = false,
                Placement = PlacementMode.Top
            };

            var border = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(12),
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1)
            };

            var stack = new StackPanel();

            if (includeProfile)
            {
                var profileButton = new Button
                {
                    Content = "Profile",
                    Margin = new Thickness(0, 0, 0, 8),
                    Padding = new Thickness(10, 6, 10, 6)
                };

                stack.Children.Add(profileButton);
            }

            var logoutButton = new Button
            {
                Content = "Logout",
                Padding = new Thickness(10, 6, 10, 6)
            };

            logoutButton.Click += (_, _) => _logout?.Invoke();

            stack.Children.Add(logoutButton);
            border.Child = stack;
            popup.Child = border;

            return popup;
        }
    }
}