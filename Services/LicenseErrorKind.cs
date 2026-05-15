namespace LicenseManagement.EndUser.Avalonia.Services;

/// <summary>
/// Coarse classification of a failure from the LicenseManagement.EndUser SDK,
/// derived from the SDK's typed exception hierarchy. The view layer uses this
/// to choose a user-facing message and remediation CTA instead of branching on
/// raw <see cref="System.Exception"/> instances.
/// </summary>
public enum LicenseErrorKind
{
    None = 0,

    /// <summary>The machine could not reach the license service.</summary>
    ComputerOffline,

    /// <summary>The local license file has expired.</summary>
    LicenseExpired,

    /// <summary>The purchase receipt / subscription has expired.</summary>
    ReceiptExpired,

    /// <summary>The local license file could not be read from disk.</summary>
    CouldNotReadLicense,

    /// <summary>
    /// The license API returned an error (HTTP non-2xx). Surface
    /// the server-supplied <c>detail</c> string verbatim if present.
    /// </summary>
    ApiError,

    /// <summary>
    /// An exception type the wrapper does not recognise.
    /// Treat as transient and recoverable by retry.
    /// </summary>
    Unknown
}
