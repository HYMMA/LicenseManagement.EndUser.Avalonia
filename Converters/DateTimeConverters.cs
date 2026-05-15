using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace LicenseManagement.EndUser.Avalonia.Converters;

public sealed class UtcToLocalTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime date)
            return date.ToLocalTime().ToString("g", CultureInfo.CurrentCulture);
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}

public sealed class DateTimeToShortDateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime date)
            return date.ToLocalTime().ToString("d", CultureInfo.CurrentCulture);
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}

public sealed class NullableDateTimeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime date)
            return date.ToLocalTime().ToString("g", CultureInfo.CurrentCulture);
        return "N/A";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        BindingOperations.DoNothing;
}
