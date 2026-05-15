using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using LicenseManagement.EndUser.Avalonia.Services;
using LicenseManagement.EndUser.License;

namespace LicenseManagement.EndUser.Avalonia.Converters;

/// <summary>
/// Base class for one-way <see cref="LicenseStatusTitles"/> converters.
/// Subclasses pick a single facet from <see cref="LicenseStatusPresenter"/>.
/// </summary>
public abstract class LicenseStatusConverterBase : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LicenseStatusTitles status)
            return DefaultValue;

        return Select(LicenseStatusPresenter.For(status));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;

    protected abstract object DefaultValue { get; }
    protected abstract object Select(LicenseStatusPresentation presentation);
}

public sealed class LicenseStatusToTextConverter : LicenseStatusConverterBase
{
    protected override object DefaultValue => "Unknown";
    protected override object Select(LicenseStatusPresentation p) => p.Text;
}

public sealed class LicenseStatusToBackgroundConverter : LicenseStatusConverterBase
{
    protected override object DefaultValue => Brushes.Transparent;
    protected override object Select(LicenseStatusPresentation p) => p.Background;
}

public sealed class LicenseStatusToBorderConverter : LicenseStatusConverterBase
{
    protected override object DefaultValue => Brushes.Gray;
    protected override object Select(LicenseStatusPresentation p) => p.Foreground;
}

public sealed class LicenseStatusToIconConverter : LicenseStatusConverterBase
{
    protected override object DefaultValue => "?";
    protected override object Select(LicenseStatusPresentation p) => p.Icon;
}
