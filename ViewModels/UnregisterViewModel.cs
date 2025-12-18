using System;
using System.Windows.Input;
using Avalonia.Controls;
using LicenseManagement.EndUser.Avalonia.Commands;

namespace LicenseManagement.EndUser.Avalonia.ViewModels;

public class UnregisterViewModel : BaseViewModel
{
    private bool _isProcessing;
    private string? _errorMessage;

    public string? VendorId { get; set; }
    public string? ProductId { get; set; }
    public string? ApiKey { get; set; }
    public string? PublicKey { get; set; }

    public bool IsProcessing
    {
        get => _isProcessing;
        set => SetProperty(ref _isProcessing, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand UnregisterCommand => new RelayCommand(UnregisterLicense, CanUnregisterLicense);
    public ICommand CancelCommand => new RelayCommand(CancelAction);

    public Action? OnUnregisterComplete { get; set; }

    private bool CanUnregisterLicense(object? obj) => ApiKey != null && !IsProcessing;

    private void UnregisterLicense(object? obj)
    {
        IsProcessing = true;
        ErrorMessage = null;

        try
        {
            var pref = new PublisherPreferences(VendorId!, ProductId!, ApiKey!) { PublicKey = PublicKey };
            var context = new LicHandlingContext(pref);
            var handler = new LicenseHandlingUninstall(
                context: context,
                OnLicenseHandledSuccessfully: (c) =>
                {
                    IsProcessing = false;
                    OnUnregisterComplete?.Invoke();
                    if (obj is Window window)
                        window.Close();
                });

            handler.HandleLicense();
        }
        catch (Exception e)
        {
            IsProcessing = false;
            ErrorMessage = e.Message;
            ShowErrorView(e, obj as Control);
        }
    }

    private void CancelAction(object? obj)
    {
        if (obj is Window window)
            window.Close();
    }
}
