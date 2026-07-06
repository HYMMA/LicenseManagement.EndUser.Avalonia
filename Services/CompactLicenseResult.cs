using System;

namespace LicenseManagement.EndUser.Avalonia.Services;

/// <summary>
/// Outcome of <see cref="ILicenseService.GetCompactLicenseAsync"/>: either the
/// compact JWS token or a categorised failure.
/// </summary>
public sealed record CompactLicenseResult
{
    private CompactLicenseResult(bool success, string? token, LicenseErrorKind errorKind, Exception? exception, string correlationId)
    {
        Success = success;
        Token = token;
        ErrorKind = errorKind;
        Exception = exception;
        CorrelationId = correlationId;
    }

    public bool Success { get; }

    /// <summary>The compact JWS license (header.payload.signature); null on failure.</summary>
    public string? Token { get; }

    public LicenseErrorKind ErrorKind { get; }
    public Exception? Exception { get; }
    public string CorrelationId { get; }

    public static CompactLicenseResult Ok(string token, string correlationId) =>
        new(true, token, LicenseErrorKind.None, null, correlationId);

    public static CompactLicenseResult Fail(LicenseErrorKind kind, Exception ex, string correlationId) =>
        new(false, null, kind, ex, correlationId);
}
