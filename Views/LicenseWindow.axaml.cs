using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using LicenseManagement.EndUser.Avalonia.Services;
using LicenseManagement.EndUser.Avalonia.ViewModels;

namespace LicenseManagement.EndUser.Avalonia.Views;

public partial class LicenseWindow : Window
{
    public LicenseWindow()
    {
        InitializeComponent();
    }

    public LicenseViewModel? License
    {
        get => LicenseControlElement.License;
        set => LicenseControlElement.License = value;
    }

    public Task InitializeAsync(
        LicenseCredentials credentials,
        ObservableCollection<ProductViewModel>? products = null) =>
        LicenseControlElement.InitializeAsync(credentials, products);

    public Task InitializeAsync(
        string vendorId,
        string productId,
        string apiKey,
        string publicKey,
        uint validDays,
        ObservableCollection<ProductViewModel>? products = null) =>
        LicenseControlElement.InitializeAsync(vendorId, productId, apiKey, publicKey, validDays, products);

    /// <summary>
    /// Factory that constructs a <see cref="LicenseWindow"/> and starts the
    /// initial license validation. Awaiting the returned task lets the
    /// caller know when the embedded VM is populated.
    /// </summary>
    public static (LicenseWindow Window, Task Initialization) Create(
        LicenseCredentials credentials,
        ObservableCollection<ProductViewModel>? products = null)
    {
        var window = new LicenseWindow();
        var task = window.InitializeAsync(credentials, products);
        return (window, task);
    }
}
