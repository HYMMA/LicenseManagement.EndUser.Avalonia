# Avalonia Sample App

This sample demonstrates how to integrate `LicenseManagement.EndUser.Avalonia` into your Avalonia application.

## Features Demonstrated

1. **Embedded License Control** - Shows how to embed the `LicenseControl` UserControl directly in your application
2. **Standalone License Window** - Shows how to open the `LicenseWindow` as a dialog
3. **Feature Gating** - Demonstrates enabling/disabling features based on license status

## Running the Sample

```bash
cd samples/AvaloniaSampleApp
dotnet run
```

## Configuration

Before running with a real license, update the following constants in `MainWindow.axaml.cs`:

```csharp
private const string VendorId = "VND_01YOUR_VENDOR_ID";
private const string ProductId = "PRD_01YOUR_PRODUCT_ID";
private const string ApiKey = "your-api-key-here";
private const string PublicKey = @"<RSAKeyValue>...</RSAKeyValue>";
```

You can obtain these values from your [License Management Dashboard](https://license-management.com/dashboard).

## License Status Handling

The sample shows how to check the license status and enable/disable features accordingly:

- `LicenseStatusTitles.Valid` - Full paid license, all features enabled
- `LicenseStatusTitles.ValidTrial` - Trial mode, basic features only
- Other statuses - No license, prompt user to register
