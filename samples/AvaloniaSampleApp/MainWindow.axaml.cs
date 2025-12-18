using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using LicenseManagement.EndUser.Avalonia.ViewModels;
using LicenseManagement.EndUser.Avalonia.Views;
using LicenseManagement.EndUser.License;

namespace AvaloniaSampleApp;

public partial class MainWindow : Window
{
    // IMPORTANT: Replace these with your actual values from license-management.com
    private const string VendorId = "VND_01EXAMPLE00000000000000001";
    private const string ProductId = "PRD_01EXAMPLE00000000000000001";
    private const string ApiKey = "your-api-key-here";
    private const string PublicKey = @"<RSAKeyValue><Modulus>your-public-key-modulus</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
    private const uint ValidDays = 30;

    // Breakpoint for responsive layout switching
    private const double NarrowBreakpoint = 700;

    private readonly ObservableCollection<ProductViewModel> _products;

    public MainWindow()
    {
        InitializeComponent();

        _products = new ObservableCollection<ProductViewModel>
        {
            new ProductViewModel { Id = ProductId, Name = "Sample Product" }
        };

        // Initialize the embedded license control
        InitializeLicenseControl();

        // Subscribe to size changes for responsive layout
        this.GetObservable(BoundsProperty).Subscribe(new BoundsObserver(this));
    }

    private class BoundsObserver : IObserver<Rect>
    {
        private readonly MainWindow _window;
        public BoundsObserver(MainWindow window) => _window = window;
        public void OnCompleted() { }
        public void OnError(Exception error) { }
        public void OnNext(Rect value) => _window.UpdateLayout(value.Width);
    }

    private void UpdateLayout(double width)
    {
        var isNarrow = width < NarrowBreakpoint;

        WideLayout.IsVisible = !isNarrow;
        NarrowLayout.IsVisible = isNarrow;

        // Sync license data between layouts
        if (isNarrow)
        {
            EmbeddedLicenseControlNarrow.License = EmbeddedLicenseControl.License;
        }

        // Update button states for active layout
        UpdateFeatureButtons();
    }

    private void InitializeLicenseControl()
    {
        // In a real application, use your actual credentials
        // For demo purposes, we'll create a mock license view model
        var mockLicense = new LicenseViewModel
        {
            VendorId = VendorId,
            VendorName = "Sample Vendor",
            ApiKey = ApiKey,
            PublicKey = PublicKey,
            ValidDays = ValidDays,
            Products = _products,
            Product = _products[0],
            ComputerName = System.Environment.MachineName,
            Status = LicenseStatusTitles.ValidTrial,
            TrialExpires = System.DateTime.UtcNow.AddDays(14),
            Expires = System.DateTime.UtcNow.AddDays(30),
        };

        EmbeddedLicenseControl.License = mockLicense;
        UpdateFeatureButtons();
    }

    private void UpdateFeatureButtons()
    {
        var license = EmbeddedLicenseControl.License;
        var isPaidLicense = license?.Status == LicenseStatusTitles.Valid;
        var hasAnyLicense = license?.Status == LicenseStatusTitles.Valid ||
                           license?.Status == LicenseStatusTitles.ValidTrial;

        // Wide layout buttons
        BasicFeatureBtn.IsEnabled = hasAnyLicense;
        PremiumFeatureBtn.IsEnabled = isPaidLicense;
        ExportFeatureBtn.IsEnabled = isPaidLicense;

        // Narrow layout buttons
        BasicFeatureBtnNarrow.IsEnabled = hasAnyLicense;
        PremiumFeatureBtnNarrow.IsEnabled = isPaidLicense;
        ExportFeatureBtnNarrow.IsEnabled = isPaidLicense;
    }

    private async void OpenLicenseWindow_Click(object? sender, RoutedEventArgs e)
    {
        // Show the standalone license window
        var licenseWindow = new LicenseWindow();
        licenseWindow.License = EmbeddedLicenseControl.License;

        await licenseWindow.ShowDialog(this);

        // After the window closes, update the embedded control
        UpdateFeatureButtons();
    }

    private void RefreshLicense_Click(object? sender, RoutedEventArgs e)
    {
        EmbeddedLicenseControl.License?.CheckLicenseFile(EmbeddedLicenseControl);
        UpdateFeatureButtons();
        ShowStatus("License status refreshed!", "#DCFCE7", "#166534");
    }

    private void BasicFeature_Click(object? sender, RoutedEventArgs e)
    {
        var license = EmbeddedLicenseControl.License;
        if (license?.Status == LicenseStatusTitles.Valid ||
            license?.Status == LicenseStatusTitles.ValidTrial)
        {
            ShowStatus("Basic feature executed successfully!", "#DCFCE7", "#166534");
        }
        else
        {
            ShowStatus("Please activate your license to use this feature.", "#FEF3C7", "#92400E");
        }
    }

    private void PremiumFeature_Click(object? sender, RoutedEventArgs e)
    {
        var license = EmbeddedLicenseControl.License;
        if (license?.Status == LicenseStatusTitles.Valid)
        {
            ShowStatus("Premium feature executed successfully!", "#DCFCE7", "#166534");
        }
        else if (license?.Status == LicenseStatusTitles.ValidTrial)
        {
            ShowStatus("Premium features require a paid license. Please register your product key.", "#FEF3C7", "#92400E");
        }
        else
        {
            ShowStatus("Please activate your license to use this feature.", "#FEE2E2", "#991B1B");
        }
    }

    private void ExportFeature_Click(object? sender, RoutedEventArgs e)
    {
        var license = EmbeddedLicenseControl.License;
        if (license?.Status == LicenseStatusTitles.Valid)
        {
            ShowStatus("Data exported successfully!", "#DCFCE7", "#166534");
        }
        else
        {
            ShowStatus("Export feature requires a paid license.", "#FEF3C7", "#92400E");
        }
    }

    private void ShowStatus(string message, string bgColor, string textColor)
    {
        var bgBrush = global::Avalonia.Media.SolidColorBrush.Parse(bgColor);
        var textBrush = global::Avalonia.Media.SolidColorBrush.Parse(textColor);

        // Wide layout status
        StatusText.Text = message;
        StatusBorder.Background = bgBrush;
        StatusText.Foreground = textBrush;
        StatusBorder.IsVisible = true;

        // Narrow layout status
        StatusTextNarrow.Text = message;
        StatusBorderNarrow.Background = bgBrush;
        StatusTextNarrow.Foreground = textBrush;
        StatusBorderNarrow.IsVisible = true;
    }
}
