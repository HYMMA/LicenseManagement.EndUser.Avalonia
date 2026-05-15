using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LicenseManagement.EndUser.Avalonia.Services;
using LicenseManagement.EndUser.Avalonia.Views;
using LicenseManagement.EndUser.License;
using LicenseManagement.EndUser.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LicenseManagement.EndUser.Avalonia.ViewModels;

public sealed partial class LicenseViewModel : ObservableObject
{
    private readonly ILicenseService _licenseService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<LicenseViewModel> _logger;
    private LicenseCredentials? _credentials;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RenewLicenseFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowRegisterViewCommand))]
    [NotifyCanExecuteChangedFor(nameof(ShowUnregisterViewCommand))]
    private bool _isReady;

    [ObservableProperty]
    private DateTime? _expires;

    [ObservableProperty]
    private LicenseStatusTitles _status;

    [ObservableProperty]
    private string? _fullFileName;

    [ObservableProperty]
    private DateTime? _receiptExpires;

    [ObservableProperty]
    private string? _macAddress;

    [ObservableProperty]
    private string? _receiptCode;

    [ObservableProperty]
    private string? _message;

    [ObservableProperty]
    private string? _vendorName;

    [ObservableProperty]
    private string? _vendorId;

    [ObservableProperty]
    private DateTime? _trialExpires;

    [ObservableProperty]
    private string? _computerName;

    [ObservableProperty]
    private uint _validDays;

    [ObservableProperty]
    private ProductViewModel? _product;

    [ObservableProperty]
    private string? _customerEmail;

    [ObservableProperty]
    private ObservableCollection<ProductViewModel>? _products;

    [ObservableProperty]
    private DateTime _created;

    [ObservableProperty]
    private DateTime _updated;

    /// <summary>
    /// Designer / parameterless constructor. Use <see cref="Configure"/> to
    /// supply credentials and services before commands are invoked.
    /// </summary>
    public LicenseViewModel()
    {
        _licenseService = new LicenseService();
        _dialogService = new DialogService();
        _logger = NullLogger<LicenseViewModel>.Instance;
    }

    public LicenseViewModel(
        ILicenseService licenseService,
        IDialogService dialogService,
        ILogger<LicenseViewModel>? logger = null)
    {
        _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? NullLogger<LicenseViewModel>.Instance;
    }

    /// <summary>
    /// Bind the VM to its credentials. Called by <c>LicenseControl</c>
    /// once its <c>StyledProperty</c>s have been populated.
    /// </summary>
    public void Configure(LicenseCredentials credentials, ObservableCollection<ProductViewModel>? products)
    {
        _credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        VendorId = credentials.VendorId;
        ValidDays = credentials.ValidDays;
        if (products is not null) Products = products;
        IsReady = true;
    }

    /// <summary>
    /// API key for support / debugging. Hidden from data inspectors so it
    /// does not leak via Avalonia.Diagnostics or screen recordings.
    /// </summary>
    [Browsable(false)]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string? ApiKey => _credentials?.ApiKey;

    [Browsable(false)]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string? PublicKey => _credentials?.PublicKey;

    [Browsable(false)]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal LicenseCredentials? Credentials => _credentials;

    public static LicenseViewModel FromContext(
        LicHandlingContext context,
        ObservableCollection<ProductViewModel> products,
        ILicenseService? licenseService = null,
        IDialogService? dialogService = null,
        ILogger<LicenseViewModel>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        var vm = licenseService is null || dialogService is null
            ? new LicenseViewModel()
            : new LicenseViewModel(licenseService, dialogService, logger);

        var creds = new LicenseCredentials(
            context.PublisherPreferences.VendorId,
            context.PublisherPreferences.ProductId,
            context.PublisherPreferences.ApiKey,
            context.PublisherPreferences.PublicKey ?? string.Empty,
            context.PublisherPreferences.ValidDays);
        vm.Configure(creds, products);

        if (context.LicenseModel is not null)
            vm.ApplyModel(context.LicenseModel);

        return vm;
    }

    /// <summary>
    /// Refresh the VM from a fresh <see cref="LicenseModel"/>. Safe to call
    /// from any thread — the property writes are marshalled to the UI thread.
    /// </summary>
    public void UpdateFromLicenseModel(LicenseModel? model)
    {
        if (model is null) return;

        if (Dispatcher.UIThread.CheckAccess())
            ApplyModel(model);
        else
            Dispatcher.UIThread.Post(() => ApplyModel(model));
    }

    private void ApplyModel(LicenseModel model)
    {
        TrialExpires = model.TrialEndDate;
        Created = model.Created ?? DateTime.MinValue;
        Expires = model.Expires;
        Status = model.Status;
        Updated = model.Updated ?? DateTime.MinValue;
        ApplyReceipt(model.Receipt);
        ApplyProduct(model.Product);
        ApplyComputer(model.Computer);
    }

    private void ApplyReceipt(ReceiptModel? receipt)
    {
        if (receipt is null) return;
        ReceiptCode = receipt.Code;
        ReceiptExpires = receipt.Expires;
        CustomerEmail = receipt.BuyerEmail;
    }

    private void ApplyProduct(ProductModel? product)
    {
        if (product is null) return;
        Product = ProductViewModel.FromProductModel(product);
        VendorId = product.Vendor?.Id ?? VendorId;
        VendorName = product.Vendor?.Name ?? VendorName;
    }

    private void ApplyComputer(ComputerModel? computer)
    {
        if (computer is null) return;
        ComputerName = computer.Name ?? ComputerName;
        MacAddress = computer.MacAddress ?? MacAddress;
    }

    private bool HasCredentials() => IsReady && _credentials is not null;

    /// <summary>Re-validate the license file against the server.</summary>
    public async Task CheckLicenseFileAsync(Control? control = null)
    {
        if (_credentials is null || Product is null) return;
        var creds = _credentials.WithProductId(Product.Id ?? _credentials.ProductId);

        var result = await _licenseService.LaunchAsync(creds).ConfigureAwait(true);
        if (result.Success)
        {
            UpdateFromLicenseModel(result.Model);
        }
        else
        {
            UpdateFromLicenseModel(result.Model);
            if (result.Exception is not null)
            {
                await _dialogService.ShowErrorAsync(result.Exception, result.CorrelationId, control).ConfigureAwait(true);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(HasCredentials))]
    private async Task RenewLicenseFileAsync(object? parameter)
    {
        if (_credentials is null || Product is null) return;
        var control = parameter as Control;
        var creds = _credentials.WithProductId(Product.Id ?? _credentials.ProductId);

        var result = await _licenseService.RenewAsync(creds).ConfigureAwait(true);
        UpdateFromLicenseModel(result.Model);
        if (!result.Success && result.Exception is not null)
        {
            await _dialogService.ShowErrorAsync(result.Exception, result.CorrelationId, control).ConfigureAwait(true);
        }
    }

    [RelayCommand(CanExecute = nameof(HasCredentials))]
    private async Task ShowRegisterViewAsync(object? parameter)
    {
        if (_credentials is null) return;
        var control = parameter as Control;
        var creds = _credentials.WithProductId(Product?.Id ?? _credentials.ProductId);

        var viewModel = new RegisterLicenseViewModel(creds, _licenseService, _dialogService)
        {
            OnRegistrationComplete = model =>
            {
                UpdateFromLicenseModel(model);
                _ = CheckLicenseFileAsync(control);
            }
        };

        var view = new RegisterLicenseView { DataContext = viewModel };
        var owner = control is null ? null : TopLevel.GetTopLevel(control) as Window;
        if (owner is not null)
            await view.ShowDialog(owner).ConfigureAwait(true);
        else
            view.Show();

        await CheckLicenseFileAsync(control).ConfigureAwait(true);
    }

    [RelayCommand(CanExecute = nameof(HasCredentials))]
    private async Task ShowUnregisterViewAsync(object? parameter)
    {
        if (_credentials is null) return;
        var control = parameter as Control;
        var creds = _credentials.WithProductId(Product?.Id ?? _credentials.ProductId);

        var viewModel = new UnregisterViewModel(creds, _licenseService, _dialogService)
        {
            OnUnregisterComplete = model =>
            {
                UpdateFromLicenseModel(model);
                _ = CheckLicenseFileAsync(control);
            }
        };

        var view = new UnregisterView { DataContext = viewModel };
        var owner = control is null ? null : TopLevel.GetTopLevel(control) as Window;
        if (owner is not null)
            await view.ShowDialog(owner).ConfigureAwait(true);
        else
            view.Show();

        await CheckLicenseFileAsync(control).ConfigureAwait(true);
    }
}
