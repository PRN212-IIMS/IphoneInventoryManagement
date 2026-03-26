using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFApp.Views.Shared;

public static class UiFactory
{
    public static Brush Brush(string color) => (Brush)new BrushConverter().ConvertFromString(color)!;
    public static FontFamily Font(string name) => new(name);

    public static TextBlock Mdl2(string glyph, double size, Brush brush, Thickness? margin = null)
    {
        var icon = new TextBlock
        {
            Text = glyph,
            FontFamily = Font("Segoe MDL2 Assets"),
            FontSize = size,
            Foreground = brush,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        if (margin is not null)
        {
            icon.Margin = margin.Value;
        }

        return icon;
    }

    public static Border IconBadge(string icon, double size)
    {
        return new Border
        {
            Width = size,
            Height = size,
            CornerRadius = new CornerRadius(12),
            Background = Brush("#14BEE8"),
            Margin = new Thickness(0, 0, 16, 0),
            Child = Mdl2(icon, 20, Brushes.White)
        };
    }

    public static Button CreateButton(string text, Brush background, Brush foreground, double height, double width = 0, Thickness? margin = null)
    {
        var button = new Button
        {
            Content = text,
            Height = height,
            Background = background,
            Foreground = foreground,
            BorderThickness = new Thickness(0),
            FontFamily = Font("Bahnschrift SemiBold"),
            FontSize = 15,
            Cursor = System.Windows.Input.Cursors.Hand,
            Template = RoundedButtonTemplate(12)
        };

        if (width > 0)
        {
            button.Width = width;
        }

        if (margin is not null)
        {
            button.Margin = margin.Value;
        }

        return button;
    }

    public static Button CreateOutlineButton(string text)
    {
        return new Button
        {
            Content = text,
            Height = 52,
            Background = Brushes.White,
            BorderBrush = Brush("#D6E0EE"),
            BorderThickness = new Thickness(1.2),
            Foreground = Brush("#35507A"),
            FontFamily = Font("Bahnschrift SemiBold"),
            FontSize = 16,
            Cursor = System.Windows.Input.Cursors.Hand,
            Template = OutlineButtonTemplate(8)
        };
    }

    public static Button CreateSidebarMenuButton(string icon, string text, bool active)
    {
        var button = new Button
        {
            Height = 50,
            Margin = new Thickness(0, 0, 0, 12),
            Padding = new Thickness(18, 0, 18, 0),
            HorizontalContentAlignment = HorizontalAlignment.Left,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            Foreground = active ? Brushes.White : Brush("#93A8C6"),
            Background = active ? Brush("#0F9CC1") : Brushes.Transparent,
            FontFamily = Font("Bahnschrift SemiBold"),
            FontSize = 18,
            Template = HorizontalButtonTemplate(14)
        };

        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(Mdl2(icon, 18, active ? Brushes.White : Brush("#93A8C6"), new Thickness(0, 0, 14, 0)));
        row.Children.Add(new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center });
        button.Content = row;
        return button;
    }

    public static Button CreateAccountButton(string userName, string role, string avatar)
    {
        var button = new Button
        {
            Height = 66,
            Background = Brush("#263149"),
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Center,
            Template = HorizontalButtonTemplate(14)
        };

        var grid = new Grid { Margin = new Thickness(14, 0, 12, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        grid.Children.Add(new Border
        {
            Width = 40,
            Height = 40,
            CornerRadius = new CornerRadius(20),
            Background = Brush("#14BEE8"),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = avatar,
                Foreground = Brushes.White,
                FontFamily = Font("Bahnschrift SemiBold"),
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        });

        var textGrid = new Grid
        {
            Margin = new Thickness(14, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        textGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        textGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        textGrid.Children.Add(new TextBlock
        {
            Text = userName,
            FontFamily = Font("Bahnschrift SemiBold"),
            FontSize = 16,
            Foreground = Brushes.White,
            TextTrimming = TextTrimming.CharacterEllipsis,
            HorizontalAlignment = HorizontalAlignment.Left
        });

        var roleText = new TextBlock
        {
            Text = role,
            FontFamily = Font("Bahnschrift"),
            FontSize = 12,
            Foreground = Brush("#7E97BD"),
            Margin = new Thickness(0, 2, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetRow(roleText, 1);
        textGrid.Children.Add(roleText);

        Grid.SetColumn(textGrid, 1);
        grid.Children.Add(textGrid);

        var arrow = Mdl2("\uE70D", 14, Brush("#89A0C2"));
        arrow.Margin = new Thickness(6, 0, 0, 0);
        Grid.SetColumn(arrow, 2);
        grid.Children.Add(arrow);

        button.Content = grid;
        return button;
    }

    public static Button CreateCustomerAccountButton(string displayName)
    {
        var button = new Button
        {
            Width = 280,
            Height = 62,
            Background = Brush("#EEF4FD"),
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Center,
            Template = HorizontalButtonTemplate(31)
        };

        var content = new Grid { Margin = new Thickness(16, 0, 16, 0) };
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var avatarBadge = new Border
        {
            Width = 40,
            Height = 40,
            CornerRadius = new CornerRadius(20),
            Background = Brush("#DFE9F6"),
            VerticalAlignment = VerticalAlignment.Center,
            Child = Mdl2("\uE77B", 18, Brush("#58759D"))
        };
        Grid.SetColumn(avatarBadge, 0);
        content.Children.Add(avatarBadge);

        var nameText = new TextBlock
        {
            Text = displayName,
            Margin = new Thickness(14, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = Font("Bahnschrift SemiBold"),
            FontSize = 15,
            Foreground = Brush("#25456E"),
            TextTrimming = TextTrimming.CharacterEllipsis,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        Grid.SetColumn(nameText, 1);
        content.Children.Add(nameText);

        var arrow = Mdl2("\uE70D", 14, Brush("#758EB1"));
        Grid.SetColumn(arrow, 2);
        content.Children.Add(arrow);

        button.Content = content;
        return button;
    }
    public static FrameworkElement CreateField(string label, string icon, Control input)
    {
        var stack = new StackPanel { Margin = new Thickness(0, 24, 0, 0) };
        stack.Children.Add(new TextBlock { Text = label, FontFamily = Font("Bahnschrift SemiBold"), FontSize = 15, Foreground = Brush("#6D7E9C"), Margin = new Thickness(0, 0, 0, 10) });
        var border = new Border { Height = 52, Background = Brush("#F9FBFE"), BorderBrush = Brush("#D6E0EE"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Padding = new Thickness(14, 0, 14, 0) };
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.Children.Add(Mdl2(icon, 16, Brush("#8BA2C2"), new Thickness(0, 0, 12, 0)));
        Grid.SetColumn(input, 1);
        input.VerticalAlignment = VerticalAlignment.Center;
        if (input is PasswordBox passwordBox)
        {
            passwordBox.Padding = new Thickness(0, 8, 0, 6);
        }
        else if (input is TextBox textBox)
        {
            textBox.Padding = new Thickness(0);
        }
        grid.Children.Add(input);
        border.Child = grid;
        stack.Children.Add(border);
        return stack;
    }

    public static ControlTemplate HorizontalButtonTemplate(double radius)
    {
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(radius));
        var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
        presenter.SetBinding(ContentPresenter.HorizontalAlignmentProperty, new System.Windows.Data.Binding("HorizontalContentAlignment") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        presenter.SetBinding(ContentPresenter.VerticalAlignmentProperty, new System.Windows.Data.Binding("VerticalContentAlignment") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        presenter.SetBinding(ContentPresenter.MarginProperty, new System.Windows.Data.Binding("Padding") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        border.AppendChild(presenter);
        return new ControlTemplate(typeof(Button)) { VisualTree = border };
    }

    public static ControlTemplate RoundedButtonTemplate(double radius)
    {
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(radius));
        var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
        presenter.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        presenter.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        border.AppendChild(presenter);
        return new ControlTemplate(typeof(Button)) { VisualTree = border };
    }

    public static ControlTemplate OutlineButtonTemplate(double radius)
    {
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        border.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding("BorderBrush") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        border.SetBinding(Border.BorderThicknessProperty, new System.Windows.Data.Binding("BorderThickness") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(radius));
        var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
        presenter.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        presenter.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        border.AppendChild(presenter);
        return new ControlTemplate(typeof(Button)) { VisualTree = border };
    }
}


