using System;

namespace LicenseManagement.EndUser.Avalonia.Services;

/// <summary>
/// Immutable value object grouping the credentials required to talk to the
/// LicenseManagement.EndUser SDK. Constructing this once at the application
/// boundary keeps the API key off individual view models and prevents
/// silent mutation of credentials at runtime.
/// </summary>
public sealed record LicenseCredentials
{
    public LicenseCredentials(
        string vendorId,
        string productId,
        string apiKey,
        string publicKey,
        uint validDays)
    {
        if (string.IsNullOrWhiteSpace(vendorId))
            throw new ArgumentException("VendorId is required.", nameof(vendorId));
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("ProductId is required.", nameof(productId));
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("ApiKey is required.", nameof(apiKey));
        if (string.IsNullOrWhiteSpace(publicKey))
            throw new ArgumentException("PublicKey is required.", nameof(publicKey));

        VendorId = vendorId;
        ProductId = productId;
        ApiKey = apiKey;
        PublicKey = publicKey;
        ValidDays = validDays;
    }

    public string VendorId { get; }
    public string ProductId { get; }

    /// <summary>
    /// API key for the publisher. Treat as a secret — never log or display.
    /// </summary>
    public string ApiKey { get; }

    public string PublicKey { get; }
    public uint ValidDays { get; }

    public LicenseCredentials WithProductId(string productId) =>
        new(VendorId, productId, ApiKey, PublicKey, ValidDays);
}
