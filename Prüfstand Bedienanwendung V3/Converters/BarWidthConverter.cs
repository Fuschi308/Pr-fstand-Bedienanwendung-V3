using System;
using System.Globalization;
using System.Windows.Data;

namespace Prüfstand_Bedienanwendung_V3.Converters
{
    /// <summary>
    /// Werte: [0]=Value, [1]=Min, [2]=Max, [3]=TotalWidth -> Barbreite
    /// </summary>
    public sealed class BarWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double v = ToDouble(values, 0);
            double min = ToDouble(values, 1);
            double max = ToDouble(values, 2);
            double total = ToDouble(values, 3);

            if (total <= 0 || max <= min) return 0.0;

            double pct = (v - min) / (max - min);
            if (double.IsNaN(pct) || double.IsInfinity(pct)) pct = 0;
            pct = Math.Clamp(pct, 0, 1);

            return total * pct;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static double ToDouble(object[] a, int i)
        {
            if (a == null || a.Length <= i || a[i] == null) return 0.0;
            try { return System.Convert.ToDouble(a[i], CultureInfo.InvariantCulture); }
            catch { return 0.0; }
        }
    }
}
