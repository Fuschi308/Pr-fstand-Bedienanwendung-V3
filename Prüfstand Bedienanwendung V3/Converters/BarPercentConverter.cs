using System;
using System.Globalization;
using System.Windows.Data;

namespace Prüfstand_Bedienanwendung_V3.Converters
{
    /// <summary>
    /// values: [0]=Value, [1]=Min, [2]=Max  ->  returns 0..1 (clamped)
    /// </summary>
    public sealed class BarPercentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double v = ToDouble(values, 0);
            double min = ToDouble(values, 1);
            double max = ToDouble(values, 2);

            if (max <= min) return 0d;

            var pct = (v - min) / (max - min);
            if (double.IsNaN(pct) || double.IsInfinity(pct)) pct = 0d;
            return Math.Clamp(pct, 0d, 1d);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static double ToDouble(object[] a, int i)
        {
            if (a == null || a.Length <= i || a[i] == null) return 0d;
            try { return System.Convert.ToDouble(a[i], CultureInfo.InvariantCulture); }
            catch { return 0d; }
        }
    }
}
