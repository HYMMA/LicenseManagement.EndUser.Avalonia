using System;
using System.Threading;
using System.Threading.Tasks;
using LicenseManagement.EndUser.Exceptions;
using LicenseManagement.EndUser.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LicenseManagement.EndUser.Avalonia.Services;

/// <summary>
/// Default <see cref="ILicenseService"/> backed by the
/// LicenseManagement.EndUser SDK. All SDK calls are made via the SDK's
/// async API so the dispatcher thread never blocks on network I/O.
/// </summary>
public sealed class LicenseService : ILicenseService
{
    private readonly ILogger<LicenseService> _logger;

    public LicenseService(ILogger<LicenseService>? logger = null)
    {
        _logger = logger ?? NullLogger<LicenseService>.Instance;
    }

    public Task<LicenseOperationResult> LaunchAsync(
        LicenseCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            credentials,
            "Launch",
            context =>
            {
                var handler = new LicenseHandlingLaunch(context);
                return handler.HandleLicenseAsync();
            },
            cancellationToken);
    }

    public Task<LicenseOperationResult> RegisterAsync(
        LicenseCredentials credentials,
        string receiptCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(receiptCode))
            throw new ArgumentException("Receipt code is required.", nameof(receiptCode));

        return ExecuteAsync(
            credentials,
            "Register",
            context =>
            {
                var handler = new LicenseHandlingLaunch(
                    context,
                    OnCustomerMustEnterProductKey: () => receiptCode,
                    OnLicFileNotFound: ctx =>
                    {
                        var install = new LicenseHandlingInstall(ctx, null);
                        install.HandleLicense();
                    },
                    OnTrialValidated: () => receiptCode);
                return handler.HandleLicenseAsync();
            },
            cancellationToken);
    }

    public Task<LicenseOperationResult> RenewAsync(
        LicenseCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            credentials,
            "Renew",
            context =>
            {
                var handler = new LicenseHandlingInstall(context, null);
                return handler.HandleLicenseAsync();
            },
            cancellationToken);
    }

    public Task<LicenseOperationResult> UnregisterAsync(
        LicenseCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            credentials,
            "Unregister",
            context =>
            {
                var handler = new LicenseHandlingUninstall(context, null);
                return handler.HandleLicenseAsync();
            },
            cancellationToken);
    }

    private async Task<LicenseOperationResult> ExecuteAsync(
        LicenseCredentials credentials,
        string operation,
        Func<LicHandlingContext, Task> runHandler,
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid().ToString("n");
        var startedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "License {Operation} starting. CorrelationId={CorrelationId}, ProductId={ProductId}",
            operation, correlationId, credentials.ProductId);

        var preferences = new PublisherPreferences(credentials.VendorId, credentials.ProductId, credentials.ApiKey)
        {
            PublicKey = credentials.PublicKey,
            ValidDays = credentials.ValidDays
        };
        var context = new LicHandlingContext(preferences);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await runHandler(context).ConfigureAwait(false);

            _logger.LogInformation(
                "License {Operation} succeeded. CorrelationId={CorrelationId}, Elapsed={ElapsedMs}ms",
                operation, correlationId, (DateTime.UtcNow - startedAt).TotalMilliseconds);

            return LicenseOperationResult.Ok(context.LicenseModel, correlationId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            var kind = Classify(ex);
            _logger.LogWarning(
                ex,
                "License {Operation} failed with {ErrorKind}. CorrelationId={CorrelationId}, Elapsed={ElapsedMs}ms",
                operation, kind, correlationId, (DateTime.UtcNow - startedAt).TotalMilliseconds);

            return LicenseOperationResult.Fail(kind, ex, context.LicenseModel, correlationId);
        }
    }

    /// <summary>
    /// Map an exception to a coarse <see cref="LicenseErrorKind"/>. Some
    /// SDK exception types (<c>LicenseExpiredException</c>,
    /// <c>ReceiptExpiredException</c>) are internal to the SDK so we detect
    /// them by name rather than typed pattern matching.
    /// </summary>
    internal static LicenseErrorKind Classify(Exception ex)
    {
        return ex switch
        {
            ComputerOfflineException => LicenseErrorKind.ComputerOffline,
            CouldNotReadLicenseFromDiskException => LicenseErrorKind.CouldNotReadLicense,
            ApiException => LicenseErrorKind.ApiError,
            _ => ex.GetType().Name switch
            {
                "LicenseExpiredException" => LicenseErrorKind.LicenseExpired,
                "ReceiptExpiredException" => LicenseErrorKind.ReceiptExpired,
                _ => LicenseErrorKind.Unknown
            }
        };
    }
}
