using System;
using System.Threading;
using System.Threading.Tasks;

namespace LicenseManagement.EndUser.Avalonia.Services;

/// <summary>
/// Abstraction over the LicenseManagement.EndUser SDK so view models do not
/// instantiate <c>LicenseHandlingLaunch</c>/<c>Install</c>/<c>Uninstall</c>
/// directly. This is the single seam for unit testing the wrapper.
/// </summary>
public interface ILicenseService
{
    /// <summary>
    /// Refresh / validate the local license against the server (Launch flow).
    /// </summary>
    Task<LicenseOperationResult> LaunchAsync(
        LicenseCredentials credentials,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Register a new license using the supplied receipt code (Install flow).
    /// </summary>
    Task<LicenseOperationResult> RegisterAsync(
        LicenseCredentials credentials,
        string receiptCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renew the local license file using the credentials' existing receipt.
    /// </summary>
    Task<LicenseOperationResult> RenewAsync(
        LicenseCredentials credentials,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregister this computer from the license (Uninstall flow).
    /// </summary>
    Task<LicenseOperationResult> UnregisterAsync(
        LicenseCredentials credentials,
        CancellationToken cancellationToken = default);
}
