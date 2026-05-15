using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using LicenseManagement.EndUser.Avalonia.Services;
using LicenseManagement.EndUser.Avalonia.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LicenseManagement.EndUser.Avalonia.Views;

public partial class LicenseControl : UserControl
{
    public static readonly StyledProperty<LicenseViewModel?> LicenseProperty =
        AvaloniaProperty.Register<LicenseControl, LicenseViewModel?>(nameof(License));

    private ILicenseService _licenseService = new LicenseService();
    private IDialogService _dialogService = new DialogService();
    private ILogger<LicenseControl> _logger = NullLogger<LicenseControl>.Instance;

    static LicenseControl()
    {
        LicenseProperty.Changed.AddClassHandler<LicenseControl, LicenseViewModel?>((control, args) =>
            control.DataContext = args.NewValue.Value);
    }

    public LicenseControl()
    {
        InitializeComponent();
    }

    public LicenseViewModel? License
    {
        get => GetValue(LicenseProperty);
        set => SetValue(LicenseProperty, value);
    }

    /// <summary>
    /// Inject custom services (e.g. an <see cref="ILogger{T}"/> backed by the
    /// host application's log pipeline). Must be called before
    /// <see cref="InitializeAsync"/>.
    /// </summary>
    public void UseServices(
        ILicenseService licenseService,
        IDialogService dialogService,
        ILogger<LicenseControl>? logger = null)
    {
        _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? NullLogger<LicenseControl>.Instance;
    }

    /// <summary>
    /// Validate the license against the server and populate the embedded
    /// <see cref="License"/> view model. Safe to call from the UI thread;
    /// the network call is awaited so the dispatcher remains responsive.
    /// </summary>
    public async Task InitializeAsync(
        LicenseCredentials credentials,
        ObservableCollection<ProductViewModel>? products = null)
    {
        ArgumentNullException.ThrowIfNull(credentials);

        var products_ = products ?? new ObservableCollection<ProductViewModel>();
        var viewModel = new LicenseViewModel(_licenseService, _dialogService);
        viewModel.Configure(credentials, products_);
        SetLicenseOnUiThread(viewModel);

        var result = await _licenseService.LaunchAsync(credentials).ConfigureAwait(true);
        if (result.Model is not null)
            viewModel.UpdateFromLicenseModel(result.Model);

        if (!result.Success && result.Exception is not null)
        {
            _logger.LogWarning(result.Exception, "Initial license launch failed. CorrelationId={CorrelationId}", result.CorrelationId);
            await _dialogService.ShowErrorAsync(result.Exception, result.CorrelationId, this).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Convenience overload that builds the <see cref="LicenseCredentials"/>
    /// from raw strings.
    /// </summary>
    public Task InitializeAsync(
        string vendorId,
        string productId,
        string apiKey,
        string publicKey,
        uint validDays,
        ObservableCollection<ProductViewModel>? products = null)
    {
        var creds = new LicenseCredentials(vendorId, productId, apiKey, publicKey, validDays);
        return InitializeAsync(creds, products);
    }

    private void SetLicenseOnUiThread(LicenseViewModel viewModel)
    {
        if (Dispatcher.UIThread.CheckAccess())
            License = viewModel;
        else
            Dispatcher.UIThread.Post(() => License = viewModel);
    }
}
