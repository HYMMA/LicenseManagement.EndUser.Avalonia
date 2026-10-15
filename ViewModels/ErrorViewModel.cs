using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LicenseManagement.EndUser.Avalonia.Services;

namespace LicenseManagement.EndUser.Avalonia.ViewModels;

/// <summary>
/// View model for <c>ErrorView</c>. Surfaces a sanitised, user-facing
/// message rather than the raw exception text so internal details from
/// <c>ApiException</c> (which may include partial payloads) do not leak
/// into the UI. When the license server could not be reached it also
/// carries the ways the user can still activate this computer without it
/// (install a license file they downloaded, or open the vendor's license
/// portal to get one).
/// </summary>
public sealed partial class ErrorViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _message;

    [ObservableProperty]
    private string? _correlationId;

    /// <summary>Runs after the dialog closes; null hides the button.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanInstallLicenseFile))]
    [NotifyPropertyChangedFor(nameof(HasRecovery))]
    private Action? _installLicenseFile;

    /// <summary>Opens the vendor's license portal; null hides the button.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanOpenPortal))]
    [NotifyPropertyChangedFor(nameof(HasRecovery))]
    private Action? _openPortal;

    public ErrorViewModel()
    {
    }

    public ErrorViewModel(Exception ex, string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(ex);
        Error = ex;
        var kind = LicenseService.Classify(ex);
        Message = LicenseErrorPresenter.Describe(kind, ex);
        CorrelationId = correlationId;
    }

    /// <summary>The failure being shown; null when the dialog was built from a message alone.</summary>
    public Exception? Error { get; }

    public bool HasCorrelationId => !string.IsNullOrWhiteSpace(CorrelationId);

    public bool CanInstallLicenseFile => InstallLicenseFile is not null;

    public bool CanOpenPortal => OpenPortal is not null;

    public bool HasRecovery => CanInstallLicenseFile || CanOpenPortal;

    partial void OnCorrelationIdChanged(string? value) => OnPropertyChanged(nameof(HasCorrelationId));

    [RelayCommand]
    private void CopyCorrelationId()
    {
        if (string.IsNullOrWhiteSpace(CorrelationId)) return;
        try
        {
            var clipboard = global::Avalonia.Application.Current?.ApplicationLifetime
                is global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow?.Clipboard
                    : null;
            if (clipboard != null)
            {
                // Avalonia churned the clipboard API between 11 and 12 (SetTextAsync
                // removed, DataObject -> DataTransfer). Bind late so this compiles and
                // works on whichever Avalonia the consumer ships.
                var setText = clipboard.GetType().GetMethod("SetTextAsync", new[] { typeof(string) });
                if (setText != null)
                    _ = setText.Invoke(clipboard, new object[] { CorrelationId });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not copy correlation id: {ex.Message}");
        }
    }
}
