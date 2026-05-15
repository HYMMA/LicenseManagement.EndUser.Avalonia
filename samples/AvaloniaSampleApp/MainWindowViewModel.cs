using CommunityToolkit.Mvvm.ComponentModel;
using LicenseManagement.EndUser.License;
using LicenseManagement.EndUser.Avalonia.ViewModels;

namespace AvaloniaSampleApp;

public sealed partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPaidLicense))]
    [NotifyPropertyChangedFor(nameof(HasAnyLicense))]
    private LicenseViewModel? _license;

    [ObservableProperty]
    private bool _isNarrow;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private string? _statusBackground;

    [ObservableProperty]
    private string? _statusForeground;

    [ObservableProperty]
    private bool _isStatusVisible;

    public bool HasPaidLicense =>
        License?.Status == LicenseStatusTitles.Valid;

    public bool HasAnyLicense =>
        License?.Status == LicenseStatusTitles.Valid
        || License?.Status == LicenseStatusTitles.ValidTrial;

    partial void OnLicenseChanged(LicenseViewModel? oldValue, LicenseViewModel? newValue)
    {
        if (oldValue is not null) oldValue.PropertyChanged -= OnLicensePropertyChanged;
        if (newValue is not null) newValue.PropertyChanged += OnLicensePropertyChanged;
    }

    private void OnLicensePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LicenseViewModel.Status))
        {
            OnPropertyChanged(nameof(HasPaidLicense));
            OnPropertyChanged(nameof(HasAnyLicense));
        }
    }

    public void ShowStatus(string message, string background, string foreground)
    {
        StatusMessage = message;
        StatusBackground = background;
        StatusForeground = foreground;
        IsStatusVisible = true;
    }
}
