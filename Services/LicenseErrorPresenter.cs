using System;
using LicenseManagement.EndUser.Exceptions;

namespace LicenseManagement.EndUser.Avalonia.Services;

/// <summary>
/// Maps a categorised <see cref="LicenseErrorKind"/> to the short, sanitised
/// message the user sees. Raw exception text is never shown directly so
/// internal details from <see cref="ApiException.Message"/> cannot leak.
/// </summary>
public static class LicenseErrorPresenter
{
    public static string Describe(LicenseErrorKind kind, Exception? exception = null) => kind switch
    {
        LicenseErrorKind.ComputerOffline =>
            "We could not reach the license service. Please check your internet connection and try again.",
        LicenseErrorKind.LicenseExpired =>
            "Your license file has expired. Use \"Download New File\" to refresh it.",
        LicenseErrorKind.ReceiptExpired =>
            "Your subscription has ended. Please contact the publisher to renew it.",
        LicenseErrorKind.CouldNotReadLicense =>
            "We could not read the license file on this computer. Please reinstall the application.",
        LicenseErrorKind.ApiError =>
            "The license service returned an error. Please try again shortly. If the problem persists, contact support with the operation id below.",
        LicenseErrorKind.Unknown =>
            "An unexpected error occurred. Please try again. If the problem persists, contact support with the operation id below.",
        _ => "An unexpected error occurred."
    };
}
