using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LicenseManagement.EndUser.Avalonia.Services;
using LicenseManagement.EndUser.Avalonia.Views;
using LicenseManagement.EndUser.Exceptions;
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
    [NotifyCanExecuteChangedFor(nameof(InstallLicenseFileCommand))]
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
    /// The vendor's license portal (e.g. https://license-management.com/Portal/{slug}), where a
    /// customer can activate a computer and download its license file in a browser. Optional:
    /// when set, the dialog offers it whenever the license server can't be reached.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPortal))]
    private string? _portalUrl;

    public bool HasPortal => !string.IsNullOrWhiteSpace(PortalUrl);

    /// <summary>
    /// This computer's id as the license server knows it. The portal accepts it to activate
    /// exactly this machine when it can't connect itself.
    /// </summary>
    public string DeviceId => ComputerId.Instance.EffectiveMachineId;

    /// <summary>The folder license files live in, as users type it (Win + R).</summary>
    public string LicenseFolder => LicenseFileLocation.GetDisplayFolder(VendorId);

    partial void OnVendorIdChanged(string? value) => OnPropertyChanged(nameof(LicenseFolder));

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
                await ShowErrorAsync(result.Exception, result.CorrelationId, control).ConfigureAwait(true);
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
            await ShowErrorAsync(result.Exception, result.CorrelationId, control).ConfigureAwait(true);
        }
    }

    /// <summary>Opens <see cref="PortalUrl"/> in the default browser.</summary>
    [RelayCommand]
    public void OpenPortal()
    {
        if (!HasPortal) return;
        try
        {
            Process.Start(new ProcessStartInfo(PortalUrl!) { UseShellExecute = true });
        }
        catch (Exception e) when (e is System.ComponentModel.Win32Exception || e is InvalidOperationException)
        {
            _logger.LogWarning(e, "Could not open the license portal {PortalUrl}", PortalUrl);
        }
    }

    /// <summary>
    /// Lets the user pick a license file they downloaded or were emailed, then installs it
    /// via <see cref="ImportLicenseFileAsync"/>. The way to activate a computer that can't
    /// reach the license server.
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasCredentials))]
    private async Task InstallLicenseFileAsync(object? parameter)
    {
        if (_credentials is null) return;
        var control = parameter as Control;
        var storage = control is null ? null : TopLevel.GetTopLevel(control)?.StorageProvider;
        if (storage is null || !storage.CanOpen) return;

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = $"Install license file for {Product?.Name ?? Product?.Id ?? _credentials.ProductId}",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("License files") { Patterns = new[] { "*.lic" } },
                FilePickerFileTypes.All
            }
        }).ConfigureAwait(true);

        var path = files.Count > 0 ? files[0].TryGetLocalPath() : null;
        if (path is null) return;
        await ImportLicenseFileAsync(path, control).ConfigureAwait(true);
    }

    /// <summary>
    /// Verifies the license file at <paramref name="sourceFilePath"/> for the current product
    /// on this computer and installs it, then re-reads the license. A rejected file is shown
    /// with the SDK's reason and replaces nothing.
    /// </summary>
    public async Task ImportLicenseFileAsync(string sourceFilePath, Control? control = null)
    {
        if (_credentials is null) return;
        var creds = _credentials.WithProductId(Product?.Id ?? _credentials.ProductId);
        var preferences = new PublisherPreferences(creds.VendorId, creds.ProductId, creds.ApiKey)
        {
            PublicKey = creds.PublicKey,
            ValidDays = creds.ValidDays
        };

        var result = await Task.Run(() => LicenseFileImporter.Import(preferences, sourceFilePath)).ConfigureAwait(true);
        if (!result.Succeeded)
        {
            _logger.LogWarning("License file {Path} was not installed: {Outcome}", sourceFilePath, result.Outcome);
            await _dialogService.ShowErrorAsync(new ErrorViewModel { Message = result.Message }, control).ConfigureAwait(true);
            return;
        }
        await CheckLicenseFileAsync(control).ConfigureAwait(true);   // reads the file just installed
    }

    /// <summary>
    /// Builds what the error dialog shows. When the license server could not be reached it
    /// offers the ways to activate without it: install a license file, and open the license
    /// portal when <see cref="PortalUrl"/> is set.
    /// </summary>
    public ErrorViewModel CreateErrorViewModel(Exception exception, string? correlationId, Control? control)
    {
        var vm = new ErrorViewModel(exception, correlationId);
        if (IsUnreachable(exception))
        {
            if (_credentials is not null)
                vm.InstallLicenseFile = () => _ = InstallLicenseFileAsync(control);
            if (HasPortal)
                vm.OpenPortal = OpenPortal;
        }
        return vm;
    }

    /// <summary>Shows <paramref name="exception"/> with the recovery actions from <see cref="CreateErrorViewModel"/>.</summary>
    public Task ShowErrorAsync(Exception exception, string? correlationId, Control? control) =>
        _dialogService.ShowErrorAsync(CreateErrorViewModel(exception, correlationId, control), control);

    private static bool IsUnreachable(Exception exception)
    {
        for (var e = exception; e is not null; e = e.InnerException)
            if (e is NetworkUnavailableException || e is HttpRequestException)
                return true;
        return false;
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
