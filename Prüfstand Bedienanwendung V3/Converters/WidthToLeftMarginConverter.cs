using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Prüfstand_Bedienanwendung_V3.Converters
{
    /// <summary>
    /// Wandelt eine Breite in einen linken Margin (Thickness.Left) um.
    /// ConverterParameter: Schwelle (z.B. "0.85" für 85% der Gesamtbreite).
    /// </summary>
    public sealed class WidthToLeftMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double width = 0d;
            try { width = System.Convert.ToDouble(value, CultureInfo.InvariantCulture); } catch { }
            double factor = 0.85; // Default 85 %
            if (parameter is string s && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                factor = Math.Clamp(f, 0d, 1d);

            return new Thickness(width * factor, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
