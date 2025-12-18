using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using LicenseManagement.EndUser.Avalonia.ViewModels;

namespace LicenseManagement.EndUser.Avalonia.Views;

public partial class LicenseControl : UserControl
{
    public static readonly StyledProperty<LicenseViewModel?> LicenseProperty =
        AvaloniaProperty.Register<LicenseControl, LicenseViewModel?>(nameof(License));

    public LicenseControl()
    {
        InitializeComponent();
    }

    public LicenseViewModel? License
    {
        get => GetValue(LicenseProperty);
        set
        {
            SetValue(LicenseProperty, value);
            DataContext = value;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (License != null)
        {
            DataContext = License;
        }
    }

    public void Initialize(
        string vendorId,
        string productId,
        string apiKey,
        string publicKey,
        uint validDays,
        ObservableCollection<ProductViewModel>? products = null)
    {
        var publisher = new PublisherPreferences(vendorId, productId, apiKey)
        {
            PublicKey = publicKey,
            ValidDays = validDays
        };

        var context = new LicHandlingContext(publisher);
        var handler = new LicenseHandlingLaunch(context, OnLicenseHandledSuccessfully: model =>
        {
            var prods = products ?? new ObservableCollection<ProductViewModel>();
            License = LicenseViewModel.FromContext(context, prods);
        });

        try
        {
            handler.HandleLicense();
        }
        catch
        {
            var prods = products ?? new ObservableCollection<ProductViewModel>();
            License = LicenseViewModel.FromContext(handler.HandlingContext, prods);
        }
    }
}
