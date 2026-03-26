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
        var card = CreateBaseCard();
        var outer = new StackPanel();
        var headerGrid = CreateHeaderGrid("Account Information");
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

    public static Border CreateEditProfileCard(
        AuthenticatedUser user,
        Action onCancel,
        Action<string, string> onSave,
        string? message,
        bool isError)
    {
        var card = CreateBaseCard();
        var outer = new StackPanel();
        var headerGrid = CreateHeaderGrid("Update Personal Information", "Modify your display name and contact details.");
        outer.Children.Add(headerGrid);
        outer.Children.Add(new Border { Height = 1, Background = UiFactory.Brush("#EBF0F6") });

        var contentGrid = new Grid { Margin = new Thickness(50, 44, 50, 34) };
        contentGrid.ColumnDefinitions.Add(new ColumnDefinition());
        contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
        contentGrid.ColumnDefinitions.Add(new ColumnDefinition());
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var emailField = CreateReadOnlyField("EMAIL ADDRESS", user.Email);
        Grid.SetColumn(emailField, 0);
        Grid.SetRow(emailField, 0);
        contentGrid.Children.Add(emailField);

        var roleField = CreateReadOnlyField("ROLE", user.Role);
        Grid.SetColumn(roleField, 2);
        Grid.SetRow(roleField, 0);
        contentGrid.Children.Add(roleField);

        var statusField = CreateReadOnlyField("STATUS", user.Status);
        Grid.SetColumn(statusField, 0);
        Grid.SetRow(statusField, 1);
        contentGrid.Children.Add(statusField);

        var fullNameBox = CreateInputTextBox();
        fullNameBox.Text = user.FullName;
        var fullNameField = CreateEditableField("FULL NAME", "\uE77B", fullNameBox);
        Grid.SetColumn(fullNameField, 2);
        Grid.SetRow(fullNameField, 1);
        contentGrid.Children.Add(fullNameField);

        var phoneBox = CreateInputTextBox();
        phoneBox.Text = user.Phone;
        var phoneField = CreateEditableField("PHONE NUMBER", "\uE717", phoneBox);
        Grid.SetColumn(phoneField, 0);
        Grid.SetRow(phoneField, 2);
        contentGrid.Children.Add(phoneField);

        outer.Children.Add(contentGrid);

        var footer = new StackPanel { Margin = new Thickness(50, 0, 50, 34) };
        if (!string.IsNullOrWhiteSpace(message))
        {
            footer.Children.Add(new TextBlock
            {
                Text = message,
                FontFamily = UiFactory.Font("Bahnschrift"),
                FontSize = 14,
                Foreground = isError ? UiFactory.Brush("#D9534F") : UiFactory.Brush("#17A570"),
                Margin = new Thickness(0, 0, 0, 14),
                TextWrapping = TextWrapping.Wrap
            });
        }

        footer.Children.Add(new Border { Height = 1, Background = UiFactory.Brush("#EBF0F6"), Margin = new Thickness(0, 0, 0, 18) });
        var actionRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var cancelButton = UiFactory.CreateOutlineButton("Cancel");
        cancelButton.Width = 150;
        cancelButton.Click += (_, _) => onCancel();
        var saveButton = UiFactory.CreateButton("Save Changes", UiFactory.Brush("#0F9CC1"), Brushes.White, 54, 218, new Thickness(16, 0, 0, 0));
        saveButton.Click += (_, _) => onSave(fullNameBox.Text, phoneBox.Text);
        actionRow.Children.Add(cancelButton);
        actionRow.Children.Add(saveButton);
        footer.Children.Add(actionRow);
        outer.Children.Add(footer);

        card.Child = outer;
        return card;
    }

    private static Border CreateBaseCard()
    {
        return new Border
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
    }

    private static Grid CreateHeaderGrid(string title, string subtitle = "")
    {
        var headerGrid = new Grid { Margin = new Thickness(42, 38, 42, 32) };
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition());
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var titleStack = new StackPanel();
        titleStack.Children.Add(new TextBlock
        {
            Text = title,
            FontFamily = UiFactory.Font("Bahnschrift SemiBold"),
            FontSize = 26,
            Foreground = UiFactory.Brush("#17345C")
        });

        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            titleStack.Children.Add(new TextBlock
            {
                Text = subtitle,
                Margin = new Thickness(0, 8, 0, 0),
                FontFamily = UiFactory.Font("Bahnschrift"),
                FontSize = 15,
                Foreground = UiFactory.Brush("#6881A6")
            });
        }

        headerGrid.Children.Add(titleStack);
        return headerGrid;
    }

    private static StackPanel CreateReadOnlyField(string label, string value)
    {
        var field = new StackPanel { Margin = new Thickness(0, 0, 0, 34) };
        field.Children.Add(CreateSectionLabel(label));
        field.Children.Add(new Border
        {
            Height = 52,
            Background = UiFactory.Brush("#F9FBFE"),
            BorderBrush = UiFactory.Brush("#D6E0EE"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Margin = new Thickness(0, 12, 0, 0),
            Padding = new Thickness(22, 0, 22, 0),
            Child = new TextBlock
            {
                Text = value,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = UiFactory.Font("Bahnschrift"),
                FontSize = 17,
                Foreground = UiFactory.Brush("#6C7D98")
            }
        });
        return field;
    }

    private static StackPanel CreateEditableField(string label, string icon, TextBox textBox)
    {
        textBox.Background = Brushes.Transparent;
        textBox.BorderThickness = new Thickness(0);
        textBox.FontFamily = UiFactory.Font("Bahnschrift");
        textBox.FontSize = 17;
        textBox.Foreground = UiFactory.Brush("#17345C");
        textBox.VerticalContentAlignment = VerticalAlignment.Center;

        return UiFactory.CreateField(label, icon, textBox) as StackPanel ?? new StackPanel();
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

    private static TextBox CreateInputTextBox()
    {
        return new TextBox();
    }
}
