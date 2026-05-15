# LicenseManagement.EndUser.Avalonia

[![Build and Test](https://github.com/HYMMA/LicenseManagement.EndUser.Avalonia/actions/workflows/build.yml/badge.svg)](https://github.com/HYMMA/LicenseManagement.EndUser.Avalonia/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/LicenseManagement.EndUser.Avalonia.svg)](https://www.nuget.org/packages/LicenseManagement.EndUser.Avalonia)
[![NuGet Downloads](https://img.shields.io/nuget/dt/LicenseManagement.EndUser.Avalonia.svg)](https://www.nuget.org/packages/LicenseManagement.EndUser.Avalonia)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Avalonia UI components for [license-management.com](https://license-management.com) end-user SDK. Provides ready-to-use Window and UserControl components, view models, and converters for license registration and management workflows.

> [!IMPORTANT]
> **Account Required**: This library requires a publisher account at [license-management.com](https://license-management.com).
>
> **Free with Dev Subscription**: A developer subscription is available at no cost, which provides full access to all features for development and testing purposes.

## Features

- **Windows desktop**: Built on Avalonia 11 and the `LicenseManagement.EndUser` SDK, which depends on WMI and the Windows Registry. macOS / Linux is not currently supported.
- **Modern UI**: Clean, modern design with Fluent theme and vector iconography
- **Embeddable**: Use `LicenseControl` UserControl in your existing app
- **Standalone**: Use `LicenseWindow` for a complete license management window
- **Async by default**: Non-blocking calls keep the UI responsive even on slow networks
- **Differentiated error handling**: Maps SDK exceptions (`ComputerOfflineException`, `LicenseExpiredException`, `ReceiptExpiredException`, `ApiException`, …) into user-friendly messages with a correlation id for support
- **Full functionality**: Register, unregister, and renew licenses

## Installation

```bash
dotnet add package LicenseManagement.EndUser.Avalonia
```

## Quick Start

> [!WARNING]
> Do not hardcode API keys in source. Load them from configuration files, environment variables, or platform-secure storage (Windows Credential Manager / DPAPI). The samples below use placeholders only.

### Option 1: Standalone Window

Show a license management window from anywhere in your app:

```csharp
using LicenseManagement.EndUser.Avalonia.Services;
using LicenseManagement.EndUser.Avalonia.Views;

var credentials = new LicenseCredentials(
    vendorId: "VND_01ABCDEF...",
    productId: "PRD_01ABCDEF...",
    apiKey: configuration["License:ApiKey"]!,
    publicKey: "<RSAKeyValue>...</RSAKeyValue>",
    validDays: 30);

var (licenseWindow, initialization) = LicenseWindow.Create(credentials);
await licenseWindow.ShowDialog(parentWindow);
await initialization; // optional: await the first validation
```

### Option 2: Embeddable Control

Add the license control directly to your AXAML:

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:license="clr-namespace:LicenseManagement.EndUser.Avalonia.Views;assembly=LicenseManagement.EndUser.Avalonia">

    <license:LicenseControl x:Name="LicenseControl" />

</Window>
```

Initialize asynchronously in code-behind:

```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        _ = InitializeLicenseAsync();
    }

    private async Task InitializeLicenseAsync()
    {
        var credentials = new LicenseCredentials(
            vendorId: "VND_01ABCDEF...",
            productId: "PRD_01ABCDEF...",
            apiKey: configuration["License:ApiKey"]!,
            publicKey: "<RSAKeyValue>...</RSAKeyValue>",
            validDays: 30);

        await LicenseControl.InitializeAsync(credentials);
    }
}
```

## License Status Handling

Check the license status to enable/disable features:

```csharp
using LicenseManagement.EndUser.License;

var license = LicenseControl.License;

switch (license?.Status)
{
    case LicenseStatusTitles.Valid:
        // Full access - license is valid and paid
        EnableAllFeatures();
        break;

    case LicenseStatusTitles.ValidTrial:
        // Trial mode - show limited features or trial banner
        EnableTrialFeatures();
        ShowTrialBanner(license.TrialExpires);
        break;

    case LicenseStatusTitles.InvalidTrial:
    case LicenseStatusTitles.Expired:
    case LicenseStatusTitles.ReceiptExpired:
    case LicenseStatusTitles.ReceiptUnregistered:
        // No valid license - prompt user to register
        DisableFeatures();
        ShowRegistrationPrompt();
        break;
}
```

> Trial period length is controlled by the server and embedded in the license file as `TrialEndDate`. If a trial is extended in the dashboard, the updated `TrialEndDate` will appear after the client refreshes the license file.

## Requirements

- .NET 8.0 or later
- Avalonia 11.2+

## Sample Application

See the [samples/AvaloniaSampleApp](samples/AvaloniaSampleApp) directory for a complete working example.

## Related Packages

- [LicenseManagement.Client](https://www.nuget.org/packages/LicenseManagement.Client) - Server-side SDK for managing licenses
- [LicenseManagement.EndUser.Wpf](https://www.nuget.org/packages/LicenseManagement.EndUser.Wpf) - WPF version of this package

## Documentation

For complete API documentation, visit [license-management.com/docs](https://license-management.com/docs/).

## License

MIT License - see LICENSE file for details.
