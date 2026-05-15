using Avalonia;
using Avalonia.Media;

namespace LicenseManagement.EndUser.Avalonia.Services;

/// <summary>
/// All UI-facing facets of a <c>LicenseStatusTitles</c> value, grouped so a
/// single lookup table drives badge text, colours, icon, and the visibility
/// of action buttons. Avoids the previous "switch over the enum in eight
/// converters" pattern.
/// </summary>
public sealed record LicenseStatusPresentation(
    string Text,
    string Icon,
    IBrush Background,
    IBrush Foreground,
    bool ShowRegister,
    bool ShowUnregister,
    bool ShowRenew,
    bool ShowSubscriptionDetails,
    bool ShowTrialDetails);
