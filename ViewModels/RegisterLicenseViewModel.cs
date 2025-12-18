using System;
using System.Windows.Input;
using Avalonia.Controls;
using LicenseManagement.EndUser.Avalonia.Commands;
using LicenseManagement.EndUser.Models;

namespace LicenseManagement.EndUser.Avalonia.ViewModels;

public class RegisterLicenseViewModel : BaseViewModel
{
    private string? _receiptCode;
    private bool _isProcessing;
    private string? _errorMessage;

    public string? PublicKey { get; set; }
    public string? ApiKey { get; set; }
    public string? VendorId { get; set; }
    public string? ProductId { get; set; }
    public uint ValidDays { get; set; }

    public string? ReceiptCode
    {
        get => _receiptCode;
        set => SetProperty(ref _receiptCode, value);
    }

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

    public ICommand RegisterCommand => new RelayCommand(RegisterAction, CanRegisterNewLicense);
    public ICommand CancelCommand => new RelayCommand(CancelAction);

    public Action? OnRegistrationComplete { get; set; }

    private bool CanRegisterNewLicense(object? obj) => ApiKey != null && !IsProcessing;

    private void RegisterAction(object? obj)
    {
        if (string.IsNullOrEmpty(ReceiptCode))
        {
            ErrorMessage = "Please enter a receipt code";
            return;
        }

        ErrorMessage = null;
        IsProcessing = true;

        try
        {
            var pref = new PublisherPreferences(VendorId!, ProductId!, ApiKey!)
            {
                PublicKey = PublicKey,
                ValidDays = ValidDays,
            };
            var context = new LicHandlingContext(pref);
            var handler = new LicenseHandlingLaunch(context,
                OnCustomerMustEnterProductKey: GetNewReceiptCode,
                OnLicFileNotFound: GetNewLicFile,
                OnTrialEnded: ChangeDefaultTrial,
                OnComputerUnregistered: ComputerUnregistered,
                OnTrialValidated: GetNewReceiptCode,
                OnLicenseHandledSuccessfully: (l) =>
                {
                    IsProcessing = false;
                    OnRegistrationComplete?.Invoke();
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

    private void GetNewLicFile(LicHandlingContext context)
    {
        var handler = new LicenseHandlingInstall(context, null);
        handler.HandleLicense();
    }

    private string GetNewReceiptCode() => ReceiptCode ?? string.Empty;

    private void ChangeDefaultTrial(PublisherPreferences preferences) { }

    private void ComputerUnregistered(ComputerModel model) { }

    private void CancelAction(object? obj)
    {
        if (obj is Window window)
            window.Close();
    }
}
