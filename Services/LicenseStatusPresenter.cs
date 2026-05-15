using System.Collections.Generic;
using Avalonia.Media;
using LicenseManagement.EndUser.License;

namespace LicenseManagement.EndUser.Avalonia.Services;

/// <summary>
/// Single source of truth for how each <see cref="LicenseStatusTitles"/>
/// value renders in the UI. Adding a new status means adding one entry here;
/// the converters and visibility predicates pick it up automatically.
/// </summary>
public static class LicenseStatusPresenter
{
    private static readonly LicenseStatusPresentation Unknown = new(
        Text: "Unknown",
        Icon: "?",
        Background: Brushes.Transparent,
        Foreground: Brushes.Gray,
        ShowRegister: true,
        ShowUnregister: false,
        ShowRenew: false,
        ShowSubscriptionDetails: false,
        ShowTrialDetails: false);

    private static readonly Dictionary<LicenseStatusTitles, LicenseStatusPresentation> Map = new()
    {
        [LicenseStatusTitles.Expired] = new LicenseStatusPresentation(
            Text: "License file expired.",
            Icon: "!",
            Background: SolidColorBrush.Parse("#FFF0F0"),
            Foreground: SolidColorBrush.Parse("#DC2626"),
            ShowRegister: false,
            ShowUnregister: false,
            ShowRenew: true,
            ShowSubscriptionDetails: true,
            ShowTrialDetails: false),

        [LicenseStatusTitles.Valid] = new LicenseStatusPresentation(
            Text: "Paid and active.",
            Icon: "✓",
            Background: SolidColorBrush.Parse("#F0FFF4"),
            Foreground: SolidColorBrush.Parse("#16A34A"),
            ShowRegister: false,
            ShowUnregister: true,
            ShowRenew: false,
            ShowSubscriptionDetails: true,
            ShowTrialDetails: false),

        [LicenseStatusTitles.ValidTrial] = new LicenseStatusPresentation(
            Text: "Trial and active.",
            Icon: "◷",
            Background: SolidColorBrush.Parse("#F0F9FF"),
            Foreground: SolidColorBrush.Parse("#2563EB"),
            ShowRegister: true,
            ShowUnregister: false,
            ShowRenew: false,
            ShowSubscriptionDetails: false,
            ShowTrialDetails: true),

        [LicenseStatusTitles.InvalidTrial] = new LicenseStatusPresentation(
            Text: "Trial ended and requires activation.",
            Icon: "!",
            Background: SolidColorBrush.Parse("#FFF0F0"),
            Foreground: SolidColorBrush.Parse("#DC2626"),
            ShowRegister: true,
            ShowUnregister: false,
            ShowRenew: false,
            ShowSubscriptionDetails: false,
            ShowTrialDetails: false),

        [LicenseStatusTitles.ReceiptExpired] = new LicenseStatusPresentation(
            Text: "Payment is suspended or subscription needs renewal.",
            Icon: "!",
            Background: SolidColorBrush.Parse("#FFF8E1"),
            Foreground: SolidColorBrush.Parse("#D97706"),
            ShowRegister: true,
            ShowUnregister: false,
            ShowRenew: false,
            ShowSubscriptionDetails: false,
            ShowTrialDetails: false),

        [LicenseStatusTitles.ReceiptUnregistered] = new LicenseStatusPresentation(
            Text: "Computer has been unregistered.",
            Icon: "○",
            Background: SolidColorBrush.Parse("#F5F5F5"),
            Foreground: SolidColorBrush.Parse("#6B7280"),
            ShowRegister: true,
            ShowUnregister: false,
            ShowRenew: false,
            ShowSubscriptionDetails: false,
            ShowTrialDetails: false),
    };

    public static LicenseStatusPresentation For(LicenseStatusTitles status) =>
        Map.TryGetValue(status, out var p) ? p : Unknown;
}
