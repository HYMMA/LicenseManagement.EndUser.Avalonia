using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using LicenseManagement.EndUser.License;

namespace LicenseManagement.EndUser.Avalonia.Converters;

public class LicenseStatusToTextConverter : IValueConverter
{
    private const string ComputerUnregistered = "Computer has been unregistered.";
    private const string ReceiptExpired = "Payment is suspended or subscription needs renewal.";
    private const string InvalidTrial = "Trial ended and requires activation.";
    private const string ActiveTrial = "Trial and active.";
    private const string ActivePaid = "Paid and active.";
    private const string Expired = "License file expired.";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LicenseStatusTitles status)
            return "Unknown";

        return status switch
        {
            LicenseStatusTitles.Expired => Expired,
            LicenseStatusTitles.Valid => ActivePaid,
            LicenseStatusTitles.ValidTrial => ActiveTrial,
            LicenseStatusTitles.InvalidTrial => InvalidTrial,
            LicenseStatusTitles.ReceiptExpired => ReceiptExpired,
            LicenseStatusTitles.ReceiptUnregistered => ComputerUnregistered,
            _ => "Unknown"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class LicenseStatusToBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LicenseStatusTitles status)
            return Brushes.Transparent;

        return status switch
        {
            LicenseStatusTitles.Expired => new SolidColorBrush(Color.Parse("#FFF0F0")),
            LicenseStatusTitles.Valid => new SolidColorBrush(Color.Parse("#F0FFF4")),
            LicenseStatusTitles.ValidTrial => new SolidColorBrush(Color.Parse("#F0F9FF")),
            LicenseStatusTitles.InvalidTrial => new SolidColorBrush(Color.Parse("#FFF0F0")),
            LicenseStatusTitles.ReceiptExpired => new SolidColorBrush(Color.Parse("#FFF8E1")),
            LicenseStatusTitles.ReceiptUnregistered => new SolidColorBrush(Color.Parse("#F5F5F5")),
            _ => Brushes.Transparent
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class LicenseStatusToBorderConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LicenseStatusTitles status)
            return Brushes.Gray;

        return status switch
        {
            LicenseStatusTitles.Expired => new SolidColorBrush(Color.Parse("#DC2626")),
            LicenseStatusTitles.Valid => new SolidColorBrush(Color.Parse("#16A34A")),
            LicenseStatusTitles.ValidTrial => new SolidColorBrush(Color.Parse("#2563EB")),
            LicenseStatusTitles.InvalidTrial => new SolidColorBrush(Color.Parse("#DC2626")),
            LicenseStatusTitles.ReceiptExpired => new SolidColorBrush(Color.Parse("#D97706")),
            LicenseStatusTitles.ReceiptUnregistered => new SolidColorBrush(Color.Parse("#6B7280")),
            _ => Brushes.Gray
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class LicenseStatusToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LicenseStatusTitles status)
            return "?";

        return status switch
        {
            LicenseStatusTitles.Expired => "!",
            LicenseStatusTitles.Valid => "✓",
            LicenseStatusTitles.ValidTrial => "◷",
            LicenseStatusTitles.InvalidTrial => "!",
            LicenseStatusTitles.ReceiptExpired => "!",
            LicenseStatusTitles.ReceiptUnregistered => "○",
            _ => "?"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
