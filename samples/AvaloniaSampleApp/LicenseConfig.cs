using System;
using System.IO;
using System.Text.Json;
using LicenseManagement.EndUser.Avalonia.Services;

namespace AvaloniaSampleApp;

/// <summary>
/// Loads license credentials from <c>appsettings.json</c>. In production
/// applications, prefer platform-secure storage (Windows Credential Manager
/// / DPAPI / macOS Keychain) over a plain JSON file.
/// </summary>
internal static class LicenseConfig
{
    private sealed record Root(LicenseSection? License);

    private sealed record LicenseSection(
        string? VendorId,
        string? ProductId,
        string? ApiKey,
        string? PublicKey,
        uint? ValidDays);

    public static LicenseCredentials Load(string path = "appsettings.json")
    {
        if (!File.Exists(path))
            throw new InvalidOperationException(
                $"Configuration file '{path}' not found. Copy appsettings.json next to the executable and fill in your credentials.");

        var json = File.ReadAllText(path);
        var root = JsonSerializer.Deserialize<Root>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var section = root?.License
            ?? throw new InvalidOperationException("Missing 'License' section in appsettings.json.");

        if (LooksLikePlaceholder(section.ApiKey))
            throw new InvalidOperationException(
                "appsettings.json still contains the placeholder API key. Fill in your real credentials from license-management.com.");

        return new LicenseCredentials(
            vendorId: section.VendorId ?? throw new InvalidOperationException("License:VendorId is required."),
            productId: section.ProductId ?? throw new InvalidOperationException("License:ProductId is required."),
            apiKey: section.ApiKey ?? throw new InvalidOperationException("License:ApiKey is required."),
            publicKey: section.PublicKey ?? throw new InvalidOperationException("License:PublicKey is required."),
            validDays: section.ValidDays ?? 30);
    }

    private static bool LooksLikePlaceholder(string? value) =>
        string.IsNullOrWhiteSpace(value)
        || value.Contains("your-api-key", StringComparison.OrdinalIgnoreCase)
        || value.Contains("EXAMPLE", StringComparison.OrdinalIgnoreCase);
}
