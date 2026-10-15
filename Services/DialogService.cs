using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using LicenseManagement.EndUser.Avalonia.ViewModels;
using LicenseManagement.EndUser.Avalonia.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LicenseManagement.EndUser.Avalonia.Services;

/// <summary>
/// Default <see cref="IDialogService"/>. Always logs the underlying exception
/// before any UI is shown so failures are visible to operators even if the
/// dialog is dismissed by focus changes.
/// </summary>
public sealed class DialogService : IDialogService
{
    private readonly ILogger<DialogService> _logger;

    public DialogService(ILogger<DialogService>? logger = null)
    {
        _logger = logger ?? NullLogger<DialogService>.Instance;
    }

    public Task ShowErrorAsync(Exception exception, string? correlationId, Control? ownerControl)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return ShowErrorAsync(new ErrorViewModel(exception, correlationId), ownerControl);
    }

    public async Task ShowErrorAsync(ErrorViewModel error, Control? ownerControl)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error.Error is not null)
            _logger.LogError(error.Error, "License error surfaced to user. CorrelationId={CorrelationId}", error.CorrelationId ?? "n/a");
        else
            _logger.LogError("License error surfaced to user: {Message}. CorrelationId={CorrelationId}", error.Message, error.CorrelationId ?? "n/a");

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var errorView = new ErrorView { DataContext = error };

            var parent = ownerControl is null ? null : TopLevel.GetTopLevel(ownerControl) as Window;
            if (parent is not null)
            {
                await errorView.ShowDialog(parent).ConfigureAwait(true);
            }
            else
            {
                errorView.Show();
            }
        }).ConfigureAwait(false);
    }
}
