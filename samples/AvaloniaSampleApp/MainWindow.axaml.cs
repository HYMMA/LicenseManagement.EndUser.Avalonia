using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LicenseManagement.EndUser.Avalonia.Services;
using LicenseManagement.EndUser.Avalonia.ViewModels;
using LicenseManagement.EndUser.Avalonia.Views;
using LicenseManagement.EndUser.License;

namespace AvaloniaSampleApp;

public partial class MainWindow : Window
{
    private const double NarrowBreakpoint = 700;
    private const string SuccessBackground = "#DCFCE7";
    private const string SuccessForeground = "#166534";
    private const string WarningBackground = "#FEF3C7";
    private const string WarningForeground = "#92400E";
    private const string ErrorBackground = "#FEE2E2";
    private const string ErrorForeground = "#991B1B";

    private readonly MainWindowViewModel _viewModel = new();
    private readonly ObservableCollection<ProductViewModel> _products = new();
    private IDisposable? _boundsSubscription;
    private LicenseCredentials? _credentials;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;

        _ = InitializeLicenseAsync();

        _boundsSubscription = this.GetObservable(BoundsProperty).Subscribe(new BoundsObserver(this));
    }

    protected override void OnClosed(EventArgs e)
    {
        _boundsSubscription?.Dispose();
        _boundsSubscription = null;
        base.OnClosed(e);
    }

    private async Task InitializeLicenseAsync()
    {
        try
        {
            _credentials = LicenseConfig.Load();
            _products.Add(new ProductViewModel { Id = _credentials.ProductId, Name = "Sample Product" });

            await EmbeddedLicenseControl.InitializeAsync(_credentials, _products);
            _viewModel.License = EmbeddedLicenseControl.License;
        }
        catch (Exception ex)
        {
            // Placeholders are expected when the sample is run as-is.
            // Surface a mocked license so the UI still renders.
            ShowMockedLicense(ex.Message);
        }
    }

    private void ShowMockedLicense(string reason)
    {
        var mock = new LicenseViewModel
        {
            VendorName = "Sample Vendor",
            Products = _products,
            ComputerName = Environment.MachineName,
            Status = LicenseStatusTitles.ValidTrial,
            TrialExpires = DateTime.UtcNow.AddDays(14),
            Expires = DateTime.UtcNow.AddDays(30)
        };
        if (_products.Count == 0)
            _products.Add(new ProductViewModel { Id = "PRD_SAMPLE", Name = "Sample Product" });
        mock.Product = _products[0];
        EmbeddedLicenseControl.License = mock;
        _viewModel.License = mock;
        _viewModel.ShowStatus(
            $"Running with mocked credentials: {reason}",
            WarningBackground,
            WarningForeground);
    }

    private void UpdateLayout(double width)
    {
        _viewModel.IsNarrow = width < NarrowBreakpoint;
    }

    private sealed class BoundsObserver : IObserver<Rect>
    {
        private readonly MainWindow _window;
        public BoundsObserver(MainWindow window) => _window = window;
        public void OnCompleted() { }
        public void OnError(Exception error) { }
        public void OnNext(Rect value) => _window.UpdateLayout(value.Width);
    }

    private async void OpenLicenseWindow_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var licenseWindow = new LicenseWindow();
            if (_credentials is not null)
            {
                _ = licenseWindow.InitializeAsync(_credentials, _products);
            }
            else
            {
                licenseWindow.License = EmbeddedLicenseControl.License;
            }

            await licenseWindow.ShowDialog(this);
            _viewModel.License = EmbeddedLicenseControl.License;
        }
        catch (Exception ex)
        {
            _viewModel.ShowStatus($"Could not open license window: {ex.Message}", ErrorBackground, ErrorForeground);
        }
    }

    private async void RefreshLicense_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var license = EmbeddedLicenseControl.License;
            if (license is not null)
                await license.CheckLicenseFileAsync(EmbeddedLicenseControl);
            _viewModel.ShowStatus("License status refreshed!", SuccessBackground, SuccessForeground);
        }
        catch (Exception ex)
        {
            _viewModel.ShowStatus($"Refresh failed: {ex.Message}", ErrorBackground, ErrorForeground);
        }
    }

    private void BasicFeature_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.HasAnyLicense)
            _viewModel.ShowStatus("Basic feature executed successfully!", SuccessBackground, SuccessForeground);
        else
            _viewModel.ShowStatus("Please activate your license to use this feature.", WarningBackground, WarningForeground);
    }

    private void PremiumFeature_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.HasPaidLicense)
            _viewModel.ShowStatus("Premium feature executed successfully!", SuccessBackground, SuccessForeground);
        else if (_viewModel.HasAnyLicense)
            _viewModel.ShowStatus("Premium features require a paid license. Please register your product key.", WarningBackground, WarningForeground);
        else
            _viewModel.ShowStatus("Please activate your license to use this feature.", ErrorBackground, ErrorForeground);
    }

    private void ExportFeature_Click(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.HasPaidLicense)
            _viewModel.ShowStatus("Data exported successfully!", SuccessBackground, SuccessForeground);
        else
            _viewModel.ShowStatus("Export feature requires a paid license.", WarningBackground, WarningForeground);
    }
}
