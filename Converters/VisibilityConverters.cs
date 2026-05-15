using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using LicenseManagement.EndUser.Avalonia.Services;
using LicenseManagement.EndUser.License;

namespace LicenseManagement.EndUser.Avalonia.Converters;

/// <summary>
/// Returns <c>true</c> when the supplied <see cref="LicenseStatusTitles"/>
/// should show the matching action button. Selector is provided by
/// subclasses so the lookup remains centralised in
/// <see cref="LicenseStatusPresenter"/>.
/// </summary>
public abstract class LicenseStatusVisibilityConverterBase : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LicenseStatusTitles status)
            return false;
        return Select(LicenseStatusPresenter.For(status));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;

    protected abstract bool Select(LicenseStatusPresentation presentation);
}

public sealed class LicenseStatusToRegisterVisibilityConverter : LicenseStatusVisibilityConverterBase
{
    protected override bool Select(LicenseStatusPresentation p) => p.ShowRegister;
}

public sealed class LicenseStatusToUnregisterVisibilityConverter : LicenseStatusVisibilityConverterBase
{
    protected override bool Select(LicenseStatusPresentation p) => p.ShowUnregister;
}

public sealed class LicenseStatusToRenewVisibilityConverter : LicenseStatusVisibilityConverterBase
{
    protected override bool Select(LicenseStatusPresentation p) => p.ShowRenew;
}

public sealed class LicenseStatusToSubscriptionVisibilityConverter : LicenseStatusVisibilityConverterBase
{
    protected override bool Select(LicenseStatusPresentation p) => p.ShowSubscriptionDetails;
}

public sealed class LicenseStatusToTrialVisibilityConverter : LicenseStatusVisibilityConverterBase
{
    protected override bool Select(LicenseStatusPresentation p) => p.ShowTrialDetails;
}
