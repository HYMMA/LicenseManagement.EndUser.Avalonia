using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Controls;
using LicenseManagement.EndUser.Avalonia.Commands;
using LicenseManagement.EndUser.Avalonia.Views;
using LicenseManagement.EndUser.License;
using LicenseManagement.EndUser.Models;

namespace LicenseManagement.EndUser.Avalonia.ViewModels;

public class LicenseViewModel : BaseViewModel
{
    private DateTime? _expires;
    private LicenseStatusTitles _status;
    private string? _fullFileName;
    private DateTime? _receiptExpires;
    private string? _macAddress;
    private string? _receiptCode;
    private string? _message;
    private string? _vendorName;
    private string? _vendorId;
    private DateTime? _trialExpires;
    private string? _computerName;
    private uint _validDays;
    private ProductViewModel? _product;
    private string? _customerEmail;
    private ObservableCollection<ProductViewModel>? _products;

    public static LicenseViewModel FromContext(LicHandlingContext context, ObservableCollection<ProductViewModel> products)
    {
        var lic = new LicenseViewModel
        {
            ValidDays = context.PublisherPreferences.ValidDays,
            VendorId = context.PublisherPreferences.VendorId,
            PublicKey = context.PublisherPreferences.PublicKey,
            ApiKey = context.PublisherPreferences.ApiKey,
            TrialExpires = context.LicenseModel.TrialEndDate,
            Created = context.LicenseModel.Created ?? DateTime.MinValue,
            Expires = context.LicenseModel.Expires,
            Updated = context.LicenseModel.Updated ?? DateTime.MinValue,
            Status = context.LicenseModel.Status
        };

        if (context.LicenseModel.Receipt != null)
        {
            lic.ReceiptCode = context.LicenseModel.Receipt.Code;
            lic.ReceiptExpires = context.LicenseModel.Receipt.Expires;
            lic.CustomerEmail = context.LicenseModel.Receipt.BuyerEmail;
        }

        if (context.LicenseModel.Product != null)
        {
            lic.Product = ProductViewModel.FromProductModel(context.LicenseModel.Product);
            lic.VendorName = context.LicenseModel.Product?.Vendor?.Name;
        }

        if (context.LicenseModel.Computer != null)
        {
            lic.ComputerName = context.LicenseModel.Computer?.Name;
            lic.MacAddress = context.LicenseModel.Computer?.MacAddress;
        }

        lic.Products = products;
        return lic;
    }

    public string? FullFileName
    {
        get => _fullFileName;
        set => SetProperty(ref _fullFileName, value);
    }

    public string? CustomerEmail
    {
        get => _customerEmail;
        set => SetProperty(ref _customerEmail, value);
    }

    public DateTime? Expires
    {
        get => _expires;
        set => SetProperty(ref _expires, value);
    }

    public DateTime? ReceiptExpires
    {
        get => _receiptExpires;
        set => SetProperty(ref _receiptExpires, value);
    }

    public DateTime? TrialExpires
    {
        get => _trialExpires;
        set => SetProperty(ref _trialExpires, value);
    }

    public string? ComputerName
    {
        get => _computerName;
        set => SetProperty(ref _computerName, value);
    }

    public string? MacAddress
    {
        get => _macAddress;
        set => SetProperty(ref _macAddress, value);
    }

    public string? VendorName
    {
        get => _vendorName;
        set => SetProperty(ref _vendorName, value);
    }

    public ObservableCollection<ProductViewModel>? Products
    {
        get => _products;
        set => SetProperty(ref _products, value);
    }

    public ProductViewModel? Product
    {
        get => _product;
        set
        {
            if (_product == null || _product.Id != value?.Id)
            {
                SetProperty(ref _product, value);
            }
        }
    }

    public string? VendorId
    {
        get => _vendorId;
        set => SetProperty(ref _vendorId, value);
    }

    public string? ReceiptCode
    {
        get => _receiptCode;
        set => SetProperty(ref _receiptCode, value);
    }

    public string? Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public uint ValidDays
    {
        get => _validDays;
        set => SetProperty(ref _validDays, value);
    }

    public string? ApiKey { get; set; }

    public LicenseStatusTitles Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string? PublicKey { get; set; }

    public ICommand ShowRegisterViewCommand => new RelayCommand(ShowRegisterWindow, _ => true);
    public ICommand ShowUnregisterViewCommand => new RelayCommand(ShowUnregisterWindow, _ => true);
    public ICommand RenewLicenseFileCommand => new RelayCommand(RenewLicenseFileAction, _ => ApiKey != null);

    public void CheckLicenseFile(Control? control = null)
    {
        if (Product == null) return;

        var publisher = new PublisherPreferences(VendorId!, Product.Id!, ApiKey!)
        {
            ValidDays = ValidDays,
            PublicKey = PublicKey
        };

        var context = new LicHandlingContext(publisher);
        var handler = new LicenseHandlingLaunch(context, OnLicenseHandledSuccessfully: UpdateFromLicenseModel);

        try
        {
            handler.HandleLicense();
        }
        catch (Exception)
        {
            UpdateFromLicenseModel(handler.HandlingContext.LicenseModel);
            if (context.Exception != null)
                ShowErrorView(context.Exception, control);
        }
    }

    internal void RenewLicenseFileAction(object? obj)
    {
        if (Product == null) return;

        var publisher = new PublisherPreferences(VendorId!, Product.Id!, ApiKey!)
        {
            ValidDays = ValidDays,
            PublicKey = PublicKey
        };

        var context = new LicHandlingContext(publisher);
        var handler = new LicenseHandlingInstall(context, UpdateFromLicenseModel);

        try
        {
            handler.HandleLicense();
        }
        catch (Exception e)
        {
            ShowErrorView(e, obj as Control);
        }
    }

    internal void UpdateFromLicenseModel(LicenseModel model)
    {
        TrialExpires = model.TrialEndDate;
        Created = model.Created ?? DateTime.MinValue;
        Expires = model.Expires;
        Status = model.Status;
        MacAddress = model.Computer?.MacAddress ?? MacAddress;
        ComputerName = model.Computer?.Name ?? ComputerName;
        VendorId = model.Product?.Vendor?.Id ?? VendorId;
        VendorName = model.Product?.Vendor?.Name ?? VendorName;
        Product = ProductViewModel.FromProductModel(model.Product) ?? Product;
        Updated = model.Updated ?? DateTime.MinValue;

        if (model.Receipt != null)
        {
            ReceiptCode = model.Receipt.Code;
            ReceiptExpires = model.Receipt.Expires;
            CustomerEmail = model.Receipt.BuyerEmail;
        }
    }

    private async void ShowRegisterWindow(object? obj)
    {
        var control = obj as Control;
        var viewModel = new RegisterLicenseViewModel
        {
            ApiKey = ApiKey,
            ProductId = Product?.Id,
            VendorId = VendorId,
            PublicKey = PublicKey,
            ValidDays = ValidDays,
        };

        var view = new RegisterLicenseView { DataContext = viewModel };

        viewModel.OnRegistrationComplete = () => CheckLicenseFile(control);

        var topLevel = control != null ? TopLevel.GetTopLevel(control) : null;
        if (topLevel is Window parentWindow)
        {
            await view.ShowDialog(parentWindow);
        }
        else
        {
            view.Show();
        }

        CheckLicenseFile(control);
    }

    private async void ShowUnregisterWindow(object? obj)
    {
        var control = obj as Control;
        var viewModel = new UnregisterViewModel
        {
            ApiKey = ApiKey,
            ProductId = Product?.Id,
            VendorId = VendorId,
            PublicKey = PublicKey,
        };

        var view = new UnregisterView { DataContext = viewModel };

        viewModel.OnUnregisterComplete = () => CheckLicenseFile(control);

        var topLevel = control != null ? TopLevel.GetTopLevel(control) : null;
        if (topLevel is Window parentWindow)
        {
            await view.ShowDialog(parentWindow);
        }
        else
        {
            view.Show();
        }

        CheckLicenseFile(control);
    }
}
