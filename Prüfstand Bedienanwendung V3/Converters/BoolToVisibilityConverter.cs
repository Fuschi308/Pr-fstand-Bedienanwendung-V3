using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Prüfstand_Bedienanwendung_V3.Converters
{
    /// <summary>
    /// Eigener Bool->Visibility Konverter (vermeidet Designerprobleme mit dem BCL-Typ).
    /// Parameter-Optionen:
    /// - "Invert"  : invertiert true/false
    /// - "Hidden"  : verwendet Hidden statt Collapsed
    /// - "InvertHidden": beides
    /// </summary>
    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;

            if (value is bool b) flag = b;
            else if (value is bool?) flag = ((bool?)value).GetValueOrDefault();

            var param = (parameter as string)?.ToLowerInvariant();

            bool invert = param?.Contains("invert") == true;
            bool useHidden = param?.Contains("hidden") == true;

            if (invert) flag = !flag;

            if (flag) return Visibility.Visible;
            return useHidden ? Visibility.Hidden : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
            {
                bool result = v == Visibility.Visible;
                var param = (parameter as string)?.ToLowerInvariant();
                bool invert = param?.Contains("invert") == true;
                return invert ? !result : result;
            }
            return false;
        }
    }
}
