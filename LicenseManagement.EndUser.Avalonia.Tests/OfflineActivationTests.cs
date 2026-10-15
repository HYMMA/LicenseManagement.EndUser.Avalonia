using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using LicenseManagement.EndUser.Avalonia.Services;
using LicenseManagement.EndUser.Avalonia.ViewModels;
using LicenseManagement.EndUser.Exceptions;
using Xunit;

namespace LicenseManagement.EndUser.Avalonia.Tests;

/// <summary>
/// The offline way to activate: when the license server can't be reached the error dialog
/// offers "Install license file…" and, if the vendor configured a portal, "Open license
/// portal". Expectations come from the rules of the feature, not from the implementation:
/// recovery only for network failures (NetworkUnavailableException / HttpRequestException
/// anywhere in the chain), the portal only when PortalUrl is set, and the SDK's own
/// end-user messages shown as they are.
/// </summary>
public class OfflineActivationTests
{
    private static LicenseCredentials DefaultCreds() =>
        new("VDR_01", "PRD_01", "api-key", "public-key", validDays: 90);

    private static LicenseViewModel ConfiguredVm(RecordingLicenseService? service = null, RecordingDialogService? dialogs = null)
    {
        var vm = new LicenseViewModel(service ?? new RecordingLicenseService(), dialogs ?? new RecordingDialogService());
        vm.Configure(DefaultCreds(), products: null);
        return vm;
    }

    private static NetworkUnavailableException DnsFailure() =>
        new("This computer's network could not find license-management.com. Ask your IT team to allow it, or install the license file by hand.",
            new HttpRequestException("No such host is known."),
            NetworkFailureReason.NameResolution,
            "license-management.com");

    // ── recovery actions ─────────────────────────────────────────────────────

    [Fact]
    public void DnsFailure_OffersInstallLicenseFile()
    {
        var error = ConfiguredVm().CreateErrorViewModel(DnsFailure(), "corr", control: null);

        Assert.True(error.CanInstallLicenseFile);
        Assert.NotNull(error.InstallLicenseFile);
        Assert.True(error.HasRecovery);
    }

    [Fact]
    public void UnrelatedFailure_OffersNoRecovery()
    {
        var vm = ConfiguredVm();
        vm.PortalUrl = "https://license-management.com/Portal/acme";

        var error = vm.CreateErrorViewModel(new InvalidOperationException("boom"), "corr", control: null);

        Assert.False(error.CanInstallLicenseFile);
        Assert.False(error.CanOpenPortal);
        Assert.False(error.HasRecovery);
    }

    [Fact]
    public void HttpRequestException_DeepInTheChain_CountsAsUnreachable()
    {
        var wrapped = new Exception("outer", new InvalidOperationException("middle", new HttpRequestException("socket")));

        var error = ConfiguredVm().CreateErrorViewModel(wrapped, null, control: null);

        Assert.True(error.CanInstallLicenseFile);
    }

    [Fact]
    public void NetworkUnavailable_WrappedInAnotherException_CountsAsUnreachable()
    {
        var wrapped = new Exception("outer", DnsFailure());

        var error = ConfiguredVm().CreateErrorViewModel(wrapped, null, control: null);

        Assert.True(error.HasRecovery);
    }

    [Fact]
    public void Portal_NotOffered_WhenPortalUrlNotSet()
    {
        var error = ConfiguredVm().CreateErrorViewModel(DnsFailure(), null, control: null);

        Assert.False(error.CanOpenPortal);
        Assert.Null(error.OpenPortal);
    }

    [Fact]
    public void Portal_Offered_WhenPortalUrlSet()
    {
        var vm = ConfiguredVm();
        vm.PortalUrl = "https://license-management.com/Portal/acme";

        var error = vm.CreateErrorViewModel(DnsFailure(), null, control: null);

        Assert.True(error.CanOpenPortal);
        Assert.True(error.CanInstallLicenseFile);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("https://license-management.com/Portal/acme", true)]
    public void HasPortal_FollowsPortalUrl(string? url, bool expected)
    {
        var vm = ConfiguredVm();
        vm.PortalUrl = url;
        Assert.Equal(expected, vm.HasPortal);
    }

    [Fact]
    public void HasPortal_RaisesPropertyChanged_WhenPortalUrlChanges()
    {
        var vm = ConfiguredVm();
        var fired = false;
        vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(vm.HasPortal)) fired = true; };

        vm.PortalUrl = "https://license-management.com/Portal/acme";

        Assert.True(fired);
    }

    [Fact]
    public void ErrorViewModel_WithoutActions_HasNoRecovery()
    {
        var error = new ErrorViewModel(DnsFailure());
        Assert.False(error.HasRecovery);
    }

    // ── messages ─────────────────────────────────────────────────────────────

    [Fact]
    public void NetworkUnavailable_ShowsTheSdkMessageAsIs()
    {
        var ex = DnsFailure();

        var error = new ErrorViewModel(ex, "corr");

        Assert.Equal(ex.Message, error.Message);
        Assert.Same(ex, error.Error);
    }

    [Fact]
    public void Describe_NetworkUnavailable_WithoutException_StillSaysSomething()
    {
        var msg = LicenseErrorPresenter.Describe(LicenseErrorKind.NetworkUnavailable);
        Assert.False(string.IsNullOrWhiteSpace(msg));
    }

    [Fact]
    public void LicenseFolder_IsTheFolderUsersTypeIntoRun()
    {
        Assert.Equal(@"%localappdata%\License-Management.com\VDR_01", ConfiguredVm().LicenseFolder);
    }

    // ── installing a file ────────────────────────────────────────────────────

    [Fact]
    public async Task ImportLicenseFile_RejectedFile_ShowsImporterReason_AndDoesNotRelaunch()
    {
        var service = new RecordingLicenseService();
        var dialogs = new RecordingDialogService();
        var vm = ConfiguredVm(service, dialogs);
        var path = Path.Combine(Path.GetTempPath(), $"not-a-license-{Guid.NewGuid():n}.lic");
        await File.WriteAllTextAsync(path, "this is not a license file");
        try
        {
            await vm.ImportLicenseFileAsync(path);
        }
        finally
        {
            File.Delete(path);
        }

        var shown = Assert.Single(dialogs.Shown);
        Assert.Contains("isn't a license file", shown.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(shown.Error);                 // the importer's own words, not a generic sanitised text
        Assert.False(shown.HasRecovery);
        Assert.Equal(0, service.LaunchCalls);
    }

    [Fact]
    public async Task ShowErrorAsync_SendsRecoveryActionsToTheDialog()
    {
        var dialogs = new RecordingDialogService();
        var vm = ConfiguredVm(dialogs: dialogs);
        vm.PortalUrl = "https://license-management.com/Portal/acme";

        await vm.ShowErrorAsync(DnsFailure(), "corr", control: null);

        var shown = Assert.Single(dialogs.Shown);
        Assert.True(shown.CanInstallLicenseFile);
        Assert.True(shown.CanOpenPortal);
        Assert.Equal("corr", shown.CorrelationId);
    }

    [Fact]
    public async Task LaunchFailure_FromCheckLicenseFile_ReachesTheDialogWithRecovery()
    {
        var service = new RecordingLicenseService { LaunchResult = LicenseOperationResult.Fail(LicenseErrorKind.NetworkUnavailable, DnsFailure(), null, "corr") };
        var dialogs = new RecordingDialogService();
        var vm = ConfiguredVm(service, dialogs);
        vm.Product = new ProductViewModel { Id = "PRD_01", Name = "Product" };

        await vm.CheckLicenseFileAsync();

        var shown = Assert.Single(dialogs.Shown);
        Assert.True(shown.CanInstallLicenseFile);
    }

    [Fact]
    public async Task CustomDialogService_ImplementingOnlyTheOldMethod_StillGetsTheError()
    {
        var legacy = new LegacyDialogService();
        var error = new ErrorViewModel(DnsFailure(), "corr");

        await ((IDialogService)legacy).ShowErrorAsync(error, null);

        Assert.Same(error.Error, legacy.LastException);
        Assert.Equal("corr", legacy.LastCorrelationId);
    }

    // ── stubs ────────────────────────────────────────────────────────────────

    private sealed class RecordingLicenseService : ILicenseService
    {
        public int LaunchCalls;
        public LicenseOperationResult LaunchResult { get; set; } = LicenseOperationResult.Ok(null, "stub");

        public Task<LicenseOperationResult> LaunchAsync(LicenseCredentials c, CancellationToken ct = default)
        {
            LaunchCalls++;
            return Task.FromResult(LaunchResult);
        }

        public Task<LicenseOperationResult> RegisterAsync(LicenseCredentials c, string code, CancellationToken ct = default)
            => Task.FromResult(LicenseOperationResult.Ok(null, "stub"));

        public Task<LicenseOperationResult> RenewAsync(LicenseCredentials c, CancellationToken ct = default)
            => Task.FromResult(LicenseOperationResult.Ok(null, "stub"));

        public Task<LicenseOperationResult> UnregisterAsync(LicenseCredentials c, CancellationToken ct = default)
            => Task.FromResult(LicenseOperationResult.Ok(null, "stub"));

        public Task<CompactLicenseResult> GetCompactLicenseAsync(LicenseCredentials c, global::LicenseManagement.EndUser.LicenseFormat format, CancellationToken ct = default)
            => Task.FromResult(CompactLicenseResult.Ok("stub.jws.token", "stub"));
    }

    private sealed class RecordingDialogService : IDialogService
    {
        public List<ErrorViewModel> Shown { get; } = new();

        public Task ShowErrorAsync(Exception exception, string? correlationId, Control? ownerControl)
        {
            Shown.Add(new ErrorViewModel(exception, correlationId));
            return Task.CompletedTask;
        }

        public Task ShowErrorAsync(ErrorViewModel error, Control? ownerControl)
        {
            Shown.Add(error);
            return Task.CompletedTask;
        }
    }

    private sealed class LegacyDialogService : IDialogService
    {
        public Exception? LastException;
        public string? LastCorrelationId;

        public Task ShowErrorAsync(Exception exception, string? correlationId, Control? ownerControl)
        {
            LastException = exception;
            LastCorrelationId = correlationId;
            return Task.CompletedTask;
        }
    }
}
