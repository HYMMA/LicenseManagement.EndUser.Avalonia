using System;
using LicenseManagement.EndUser.Models;

namespace LicenseManagement.EndUser.Avalonia.Services;

/// <summary>
/// Outcome of an <see cref="ILicenseService"/> operation. Encapsulates either
/// a successful <see cref="LicenseModel"/> or a categorised failure so view
/// models can branch on <see cref="ErrorKind"/> without try/catch.
/// </summary>
public sealed record LicenseOperationResult
{
    private LicenseOperationResult(
        bool success,
        LicenseModel? model,
        LicenseErrorKind errorKind,
        Exception? exception,
        string correlationId)
    {
        Success = success;
        Model = model;
        ErrorKind = errorKind;
        Exception = exception;
        CorrelationId = correlationId;
    }

    public bool Success { get; }
    public LicenseModel? Model { get; }
    public LicenseErrorKind ErrorKind { get; }
    public Exception? Exception { get; }

    /// <summary>
    /// Correlation id generated locally for the operation. Surface this in
    /// support-facing UI so customer reports can be joined against server logs
    /// (the webapp echoes <c>X-Correlation-Id</c> for inbound requests).
    /// </summary>
    public string CorrelationId { get; }

    public static LicenseOperationResult Ok(LicenseModel? model, string correlationId) =>
        new(true, model, LicenseErrorKind.None, null, correlationId);

    public static LicenseOperationResult Fail(LicenseErrorKind kind, Exception ex, LicenseModel? partialModel, string correlationId) =>
        new(false, partialModel, kind, ex, correlationId);
}
