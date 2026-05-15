using LicenseManagement.EndUser.Avalonia.Services;
using Xunit;

namespace LicenseManagement.EndUser.Avalonia.Tests;

public class LicenseCredentialsTests
{
    private const string V = "VDR_01";
    private const string P = "PRD_01";
    private const string A = "api-key-value";
    private const string K = "public-key-value";

    [Theory]
    [InlineData("",  P,  A,  K)]
    [InlineData(" ", P,  A,  K)]
    [InlineData(V,  "",  A,  K)]
    [InlineData(V,  " ", A,  K)]
    [InlineData(V,   P, "",  K)]
    [InlineData(V,   P, " ", K)]
    [InlineData(V,   P,  A, "")]
    [InlineData(V,   P,  A, " ")]
    public void Constructor_Throws_WhenRequiredArgIsBlank(
        string vendorId, string productId, string apiKey, string publicKey)
    {
        Assert.Throws<ArgumentException>(() =>
            new LicenseCredentials(vendorId, productId, apiKey, publicKey, validDays: 90));
    }

    [Fact]
    public void Constructor_Succeeds_WithAllRequiredArgs()
    {
        var creds = new LicenseCredentials(V, P, A, K, validDays: 30);

        Assert.Equal(V,  creds.VendorId);
        Assert.Equal(P,  creds.ProductId);
        Assert.Equal(A,  creds.ApiKey);
        Assert.Equal(K,  creds.PublicKey);
        Assert.Equal(30u, creds.ValidDays);
    }

    [Fact]
    public void WithProductId_CreatesNewInstance_PreservingOtherFields()
    {
        var original = new LicenseCredentials(V, P, A, K, validDays: 90);
        var updated  = original.WithProductId("PRD_02");

        Assert.Equal("PRD_02", updated.ProductId);
        Assert.Equal(V,  updated.VendorId);
        Assert.Equal(A,  updated.ApiKey);
        Assert.Equal(K,  updated.PublicKey);
        Assert.Equal(90u, updated.ValidDays);
        Assert.NotSame(original, updated);
    }

    [Fact]
    public void WithProductId_DoesNotMutateOriginal()
    {
        var original = new LicenseCredentials(V, P, A, K, validDays: 90);
        _ = original.WithProductId("PRD_99");

        Assert.Equal(P, original.ProductId);
    }
}
