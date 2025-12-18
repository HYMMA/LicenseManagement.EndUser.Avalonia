using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LicenseManagement.EndUser.Avalonia.Views;

public partial class ErrorView : Window
{
    public ErrorView()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
