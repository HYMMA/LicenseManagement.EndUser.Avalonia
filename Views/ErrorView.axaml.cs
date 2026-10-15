using Avalonia.Controls;
using Avalonia.Interactivity;
using LicenseManagement.EndUser.Avalonia.ViewModels;

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

    private void OpenPortalButton_Click(object? sender, RoutedEventArgs e)
    {
        // the browser opens alongside; keep this dialog so the user can come back to Install
        (DataContext as ErrorViewModel)?.OpenPortal?.Invoke();
    }

    private void InstallFileButton_Click(object? sender, RoutedEventArgs e)
    {
        // close first: the file picker and the refreshed license belong to the license window
        var install = (DataContext as ErrorViewModel)?.InstallLicenseFile;
        Close();
        install?.Invoke();
    }
}
