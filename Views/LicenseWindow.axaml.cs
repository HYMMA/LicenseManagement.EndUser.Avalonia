using System.Collections.ObjectModel;
using Avalonia.Controls;
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

    public void Initialize(
        string vendorId,
        string productId,
        string apiKey,
        string publicKey,
        uint validDays,
        ObservableCollection<ProductViewModel>? products = null)
    {
        LicenseControlElement.Initialize(vendorId, productId, apiKey, publicKey, validDays, products);
    }

    public static LicenseWindow Create(
        string vendorId,
        string productId,
        string apiKey,
        string publicKey,
        uint validDays,
        ObservableCollection<ProductViewModel>? products = null)
    {
        var window = new LicenseWindow();
        window.Initialize(vendorId, productId, apiKey, publicKey, validDays, products);
        return window;
    }
}
