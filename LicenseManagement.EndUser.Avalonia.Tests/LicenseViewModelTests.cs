using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using LicenseManagement.EndUser.Avalonia.Services;
using LicenseManagement.EndUser.Avalonia.ViewModels;
using LicenseManagement.EndUser.License;
using LicenseManagement.EndUser.Models;
using Xunit;

namespace LicenseManagement.EndUser.Avalonia.Tests;

/// <summary>
/// Tests that exercise view-model behaviour without a running Avalonia application.
/// All assertions stay on the data layer (ObservableObject properties, credentials,
/// command CanExecute) and never touch the Avalonia visual tree.
/// </summary>
public class LicenseViewModelTests
{
    private static LicenseCredentials DefaultCreds() =>
        new("VDR_01", "PRD_01", "api-key", "public-key", validDays: 90);

    [Fact]
    public void Configure_SetsIsReadyTrue()
    {
        var vm = new LicenseViewModel(new StubLicenseService(), new StubDialogService());
        Assert.False(vm.IsReady);

        vm.Configure(DefaultCreds(), products: null);

        Assert.True(vm.IsReady);
    }

    [Fact]
    public void Configure_SetsVendorIdAndValidDays()
    {
        var vm = new LicenseViewModel(new StubLicenseService(), new StubDialogService());
        vm.Configure(DefaultCreds(), products: null);

        Assert.Equal("VDR_01", vm.VendorId);
        Assert.Equal(90u, vm.ValidDays);
    }

    [Fact]
    public void Configure_SetsProducts_WhenProvided()
    {
        var vm   = new LicenseViewModel(new StubLicenseService(), new StubDialogService());
        var list = new ObservableCollection<ProductViewModel>();
        vm.Configure(DefaultCreds(), products: list);

        Assert.Same(list, vm.Products);
    }

    [Fact]
    public void Configure_Throws_WhenCredentialsNull()
    {
        var vm = new LicenseViewModel(new StubLicenseService(), new StubDialogService());
        Assert.Throws<ArgumentNullException>(() => vm.Configure(null!, products: null));
    }

    [Fact]
    public void IsReady_PropertyChanged_FiresOnConfigure()
    {
        var vm    = new LicenseViewModel(new StubLicenseService(), new StubDialogService());
        var fired = false;
        vm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(vm.IsReady)) fired = true; };

        vm.Configure(DefaultCreds(), products: null);

        Assert.True(fired);
    }

    [Fact]
    public void Constructor_Throws_WhenLicenseServiceNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new LicenseViewModel(null!, new StubDialogService()));
    }

    [Fact]
    public void Constructor_Throws_WhenDialogServiceNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new LicenseViewModel(new StubLicenseService(), null!));
    }

    // ── stubs ────────────────────────────────────────────────────────────────

    private sealed class StubLicenseService : ILicenseService
    {
        public Task<LicenseOperationResult> LaunchAsync(LicenseCredentials c, CancellationToken ct = default)
            => Task.FromResult(LicenseOperationResult.Ok(null, "stub"));

        public Task<LicenseOperationResult> RegisterAsync(LicenseCredentials c, string code, CancellationToken ct = default)
            => Task.FromResult(LicenseOperationResult.Ok(null, "stub"));

        public Task<LicenseOperationResult> RenewAsync(LicenseCredentials c, CancellationToken ct = default)
            => Task.FromResult(LicenseOperationResult.Ok(null, "stub"));

        public Task<LicenseOperationResult> UnregisterAsync(LicenseCredentials c, CancellationToken ct = default)
            => Task.FromResult(LicenseOperationResult.Ok(null, "stub"));
    }

    private sealed class StubDialogService : IDialogService
    {
        public Task ShowErrorAsync(Exception exception, string? correlationId, Control? ownerControl)
            => Task.CompletedTask;
    }
}
