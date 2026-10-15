using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using LicenseManagement.EndUser.Avalonia.ViewModels;

namespace LicenseManagement.EndUser.Avalonia.Services;

/// <summary>
/// View-model–friendly seam for showing modal dialogs. Lets view models
/// surface errors without referencing <c>Views</c> types or constructing
/// windows directly. Provides correlation-id propagation for support flows.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Show the supplied error to the user. The view model is expected to
    /// pass a <paramref name="ownerControl"/> that resolves to a top-level
    /// window so the dialog appears modal.
    /// </summary>
    Task ShowErrorAsync(Exception exception, string? correlationId, Control? ownerControl);

    /// <summary>
    /// Show an error the caller has already built, including any recovery
    /// actions (install a license file, open the license portal). The default
    /// implementation keeps existing custom services working: it falls back to
    /// <see cref="ShowErrorAsync(Exception, string?, Control?)"/> and drops the
    /// recovery actions.
    /// </summary>
    Task ShowErrorAsync(ErrorViewModel error, Control? ownerControl)
    {
        ArgumentNullException.ThrowIfNull(error);
        return ShowErrorAsync(error.Error ?? new InvalidOperationException(error.Message), error.CorrelationId, ownerControl);
    }
}
