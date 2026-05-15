using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LicenseManagement.EndUser.Avalonia.Services;
using LicenseManagement.EndUser.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LicenseManagement.EndUser.Avalonia.ViewModels;

public sealed partial class UnregisterViewModel : ObservableObject
{
    private readonly ILicenseService _licenseService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<UnregisterViewModel> _logger;
    private readonly LicenseCredentials _credentials;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UnregisterCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool _isProcessing;

    [ObservableProperty]
    private string? _errorMessage;

    public UnregisterViewModel(
        LicenseCredentials credentials,
        ILicenseService licenseService,
        IDialogService dialogService,
        ILogger<UnregisterViewModel>? logger = null)
    {
        _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? NullLogger<UnregisterViewModel>.Instance;
    }

    public Action<LicenseModel?>? OnUnregisterComplete { get; set; }

    private bool CanUnregister() => !IsProcessing;

    [RelayCommand(CanExecute = nameof(CanUnregister))]
    private async Task UnregisterAsync(object? parameter)
    {
        var window = parameter as Window;
        ErrorMessage = null;
        IsProcessing = true;

        try
        {
            var result = await _licenseService.UnregisterAsync(_credentials).ConfigureAwait(true);
            if (result.Success)
            {
                _logger.LogInformation("Unregister succeeded. CorrelationId={CorrelationId}", result.CorrelationId);
                OnUnregisterComplete?.Invoke(result.Model);
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
            _logger.LogError(ex, "Unexpected error in UnregisterAsync");
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
