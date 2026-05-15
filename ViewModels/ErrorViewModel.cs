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
/// into the UI.
/// </summary>
public sealed partial class ErrorViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _message;

    [ObservableProperty]
    private string? _correlationId;

    public ErrorViewModel()
    {
    }

    public ErrorViewModel(Exception ex, string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(ex);
        var kind = LicenseService.Classify(ex);
        Message = LicenseErrorPresenter.Describe(kind, ex);
        CorrelationId = correlationId;
    }

    public bool HasCorrelationId => !string.IsNullOrWhiteSpace(CorrelationId);

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
            clipboard?.SetTextAsync(CorrelationId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Could not copy correlation id: {ex.Message}");
        }
    }
}
