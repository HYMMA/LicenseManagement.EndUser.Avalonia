using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LicenseManagement.EndUser.Avalonia.Services;
using LicenseManagement.EndUser.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LicenseManagement.EndUser.Avalonia.ViewModels;

/// <summary>
/// View model for <c>RegisterLicenseView</c>. Driven by an injected
/// <see cref="ILicenseService"/> so it can be unit-tested without spinning
/// up Avalonia and the underlying SDK.
/// </summary>
public sealed partial class RegisterLicenseViewModel : ObservableObject
{
    private readonly ILicenseService _licenseService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<RegisterLicenseViewModel> _logger;
    private readonly LicenseCredentials _credentials;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string? _receiptCode;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _isProcessing;

    [ObservableProperty]
    private string? _errorMessage;

    public RegisterLicenseViewModel(
        LicenseCredentials credentials,
        ILicenseService licenseService,
        IDialogService dialogService,
        ILogger<RegisterLicenseViewModel>? logger = null)
    {
        _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? NullLogger<RegisterLicenseViewModel>.Instance;
    }

    /// <summary>Invoked when registration succeeds. The parent VM uses this
    /// to refresh the license panel state.</summary>
    public Action<LicenseModel?>? OnRegistrationComplete { get; set; }

    private bool CanRegister() => !IsProcessing && !string.IsNullOrWhiteSpace(ReceiptCode);

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private async Task RegisterAsync(object? parameter)
    {
        var window = parameter as Window;
        ErrorMessage = null;
        IsProcessing = true;

        try
        {
            var result = await _licenseService.RegisterAsync(_credentials, ReceiptCode!).ConfigureAwait(true);

            if (result.Success)
            {
                _logger.LogInformation("Registration succeeded. CorrelationId={CorrelationId}", result.CorrelationId);
                OnRegistrationComplete?.Invoke(result.Model);
                window?.Close();
                return;
            }

            ErrorMessage = LicenseErrorPresenter.Describe(result.ErrorKind, result.Exception);
            if (result.Exception is not null)
            {
                await _dialogService.ShowErrorAsync(result.Exception, result.CorrelationId, window).ConfigureAwait(true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in RegisterAsync");
            ErrorMessage = LicenseErrorPresenter.Describe(LicenseErrorKind.Unknown, ex);
            await _dialogService.ShowErrorAsync(ex, null, window).ConfigureAwait(true);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private bool CanCancel() => !IsProcessing;

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel(object? parameter)
    {
        if (parameter is Window window)
            window.Close();
    }
}
