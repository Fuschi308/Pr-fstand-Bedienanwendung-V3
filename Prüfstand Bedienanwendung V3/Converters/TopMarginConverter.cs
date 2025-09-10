using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Prüfstand_Bedienanwendung_V3.Views.Converters
{
    /// <summary>
    /// Konvertiert eine double-Höhe in einen Thickness mit Top-Margin (0, value, 0, 0).
    /// Dient dazu, die frühere "0,{Binding ...},0,0"-Schreibweise XAML-sicher abzubilden.
    /// </summary>
    public sealed class TopMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double top = 0;
            if (value is double d && double.IsFinite(d)) top = d;
            return new Thickness(0, top, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
