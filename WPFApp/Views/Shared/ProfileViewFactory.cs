using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Services.Models;

namespace WPFApp.Views.Shared;

public static class ProfileViewFactory
{
    public static Border CreateProfileCard(
        AuthenticatedUser user,
        Action onEditProfile,
        Action onChangePassword,
        Brush roleBadgeBackground,
        Brush roleBadgeForeground)
    {
        var card = new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(20),
            BorderBrush = UiFactory.Brush("#DCE6F2"),
            BorderThickness = new Thickness(1),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 18,
                ShadowDepth = 0,
                Opacity = 0.08,
                Color = Colors.Black
            }
        };

        var outer = new StackPanel();
        var headerGrid = new Grid { Margin = new Thickness(42, 38, 42, 32) };
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition());
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var titleStack = new StackPanel();
        titleStack.Children.Add(new TextBlock
        {
            Text = "Account Information",
            FontFamily = UiFactory.Font("Bahnschrift SemiBold"),
            FontSize = 26,
            Foreground = UiFactory.Brush("#17345C")
        });
        headerGrid.Children.Add(titleStack);

        var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
        var editButton = UiFactory.CreateOutlineButton("Edit Profile");
        editButton.Width = 156;
        editButton.Click += (_, _) => onEditProfile();
        var passwordButton = UiFactory.CreateButton("Change Password", UiFactory.Brush("#0F9CC1"), Brushes.White, 54, 188, new Thickness(16, 0, 0, 0));
        passwordButton.Click += (_, _) => onChangePassword();
        actions.Children.Add(editButton);
        actions.Children.Add(passwordButton);
        Grid.SetColumn(actions, 1);
        headerGrid.Children.Add(actions);

        outer.Children.Add(headerGrid);
        outer.Children.Add(new Border { Height = 1, Background = UiFactory.Brush("#EBF0F6") });

        var detailsGrid = new Grid { Margin = new Thickness(54, 46, 54, 54) };
        detailsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        detailsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
        detailsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        detailsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        detailsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        detailsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var roleBlock = CreateProfileBadgeBlock("ROLE", user.Role, roleBadgeBackground, roleBadgeForeground);
        Grid.SetColumn(roleBlock, 0);
        Grid.SetRow(roleBlock, 0);
        detailsGrid.Children.Add(roleBlock);

        var statusBlock = CreateStatusBlock("STATUS", user.Status);
        Grid.SetColumn(statusBlock, 2);
        Grid.SetRow(statusBlock, 0);
        detailsGrid.Children.Add(statusBlock);

        var fullNameBlock = CreateProfileTextBlock("FULL NAME", user.FullName);
        Grid.SetColumn(fullNameBlock, 0);
        Grid.SetRow(fullNameBlock, 1);
        detailsGrid.Children.Add(fullNameBlock);

        var emailBlock = CreateProfileTextBlock("EMAIL ADDRESS", user.Email);
        Grid.SetColumn(emailBlock, 2);
        Grid.SetRow(emailBlock, 1);
        detailsGrid.Children.Add(emailBlock);

        var phoneBlock = CreateProfileTextBlock("PHONE NUMBER", string.IsNullOrWhiteSpace(user.Phone) ? "Not available" : user.Phone);
        Grid.SetColumn(phoneBlock, 0);
        Grid.SetRow(phoneBlock, 2);
        detailsGrid.Children.Add(phoneBlock);

        outer.Children.Add(detailsGrid);
        card.Child = outer;
        return card;
    }

    private static StackPanel CreateProfileBadgeBlock(string label, string value, Brush badgeBackground, Brush badgeForeground)
    {
        var block = new StackPanel { Margin = new Thickness(0, 0, 0, 42) };
        block.Children.Add(CreateSectionLabel(label));
        block.Children.Add(new Border
        {
            Margin = new Thickness(0, 18, 0, 0),
            Padding = new Thickness(20, 8, 20, 8),
            Background = badgeBackground,
            CornerRadius = new CornerRadius(18),
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = new TextBlock
            {
                Text = value,
                FontFamily = UiFactory.Font("Bahnschrift SemiBold"),
                FontSize = 14,
                Foreground = badgeForeground
            }
        });
        return block;
    }

    private static StackPanel CreateStatusBlock(string label, string status)
    {
        var block = new StackPanel { Margin = new Thickness(0, 0, 0, 42) };
        block.Children.Add(CreateSectionLabel(label));
        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 18, 0, 0) };
        row.Children.Add(new Border
        {
            Width = 14,
            Height = 14,
            CornerRadius = new CornerRadius(7),
            Background = status.Equals("Active", StringComparison.OrdinalIgnoreCase) ? UiFactory.Brush("#10C27D") : UiFactory.Brush("#D7DEE8"),
            Margin = new Thickness(0, 7, 10, 0)
        });
        row.Children.Add(new TextBlock
        {
            Text = status,
            FontFamily = UiFactory.Font("Bahnschrift SemiBold"),
            FontSize = 16,
            Foreground = UiFactory.Brush("#203B62")
        });
        block.Children.Add(row);
        return block;
    }

    private static StackPanel CreateProfileTextBlock(string label, string value)
    {
        var block = new StackPanel { Margin = new Thickness(0, 0, 0, 42) };
        block.Children.Add(CreateSectionLabel(label));
        block.Children.Add(new TextBlock
        {
            Text = value,
            Margin = new Thickness(0, 18, 0, 0),
            FontFamily = UiFactory.Font("Bahnschrift SemiBold"),
            FontSize = 18,
            Foreground = UiFactory.Brush("#17345C"),
            TextWrapping = TextWrapping.Wrap
        });
        return block;
    }

    private static TextBlock CreateSectionLabel(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontFamily = UiFactory.Font("Bahnschrift SemiBold"),
            FontSize = 15,
            Foreground = UiFactory.Brush("#8B9FC0")
        };
    }
}
