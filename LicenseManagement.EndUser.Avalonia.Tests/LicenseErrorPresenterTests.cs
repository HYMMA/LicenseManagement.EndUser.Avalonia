using LicenseManagement.EndUser.Avalonia.Services;
using Xunit;

namespace LicenseManagement.EndUser.Avalonia.Tests;

public class LicenseErrorPresenterTests
{
    [Theory]
    [InlineData(LicenseErrorKind.ComputerOffline)]
    [InlineData(LicenseErrorKind.LicenseExpired)]
    [InlineData(LicenseErrorKind.ReceiptExpired)]
    [InlineData(LicenseErrorKind.CouldNotReadLicense)]
    [InlineData(LicenseErrorKind.ApiError)]
    [InlineData(LicenseErrorKind.Unknown)]
    public void Describe_ReturnsNonEmptyString_ForEveryKind(LicenseErrorKind kind)
    {
        var msg = LicenseErrorPresenter.Describe(kind);
        Assert.False(string.IsNullOrWhiteSpace(msg));
    }

    [Fact]
    public void Describe_ComputerOffline_MentionsInternet()
    {
        var msg = LicenseErrorPresenter.Describe(LicenseErrorKind.ComputerOffline);
        Assert.Contains("internet", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Describe_UnknownKind_ReturnsDefaultFallback()
    {
        // Cast an out-of-range int to exercise the default switch arm.
        var msg = LicenseErrorPresenter.Describe((LicenseErrorKind)999);
        Assert.False(string.IsNullOrWhiteSpace(msg));
    }
}
