using LicenseManagement.EndUser.Avalonia.Services;
using LicenseManagement.EndUser.Models;
using Xunit;

namespace LicenseManagement.EndUser.Avalonia.Tests;

public class LicenseOperationResultTests
{
    [Fact]
    public void Ok_CreatesSuccessfulResult()
    {
        var model = new LicenseModel();
        var result = LicenseOperationResult.Ok(model, "corr-123");

        Assert.True(result.Success);
        Assert.Same(model, result.Model);
        Assert.Equal("corr-123", result.CorrelationId);
        Assert.Equal(LicenseErrorKind.None, result.ErrorKind);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void Ok_AllowsNullModel()
    {
        var result = LicenseOperationResult.Ok(null, "corr-456");

        Assert.True(result.Success);
        Assert.Null(result.Model);
    }

    [Fact]
    public void Fail_CreatesFailureResult_WithKindAndException()
    {
        var ex     = new InvalidOperationException("boom");
        var model  = new LicenseModel();
        var result = LicenseOperationResult.Fail(LicenseErrorKind.ApiError, ex, model, "corr-789");

        Assert.False(result.Success);
        Assert.Equal(LicenseErrorKind.ApiError, result.ErrorKind);
        Assert.Same(ex,    result.Exception);
        Assert.Same(model, result.Model);
        Assert.Equal("corr-789", result.CorrelationId);
    }

    [Fact]
    public void Fail_AllowsNullPartialModel()
    {
        var ex     = new Exception("oops");
        var result = LicenseOperationResult.Fail(LicenseErrorKind.Unknown, ex, null, "corr-000");

        Assert.False(result.Success);
        Assert.Null(result.Model);
    }
}
