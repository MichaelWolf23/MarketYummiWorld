using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;


namespace MarketYummiWorld.Converters;

public class StockLevelToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int s ? s > 10 ? Brushes.Green : s > 0 ? Brushes.Orange : Brushes.Red : Brushes.Gray;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ExpiryToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            var diff = (date - DateTime.Now).Days;
            if (diff < 0) return "⚠️ Просрочен";
            if (diff <= 3) return "⏳ Заканчивается";
            return "✅ В норме";
        }
        return "❓ Не указан";
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class ExpiryToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            var diff = (date - DateTime.Now).Days;
            return diff < 0 ? Brushes.Red : diff <= 3 ? Brushes.DarkOrange : Brushes.Green;
        }
        return Brushes.Gray;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
