using System;
using System.Globalization;
using System.Windows.Data;

namespace Prüfstand_Bedienanwendung_V3.Converters
{
    /// <summary>
    /// Rechnet Value/Min/Max + TotalWidth in Segmentbreiten um.
    /// ConverterParameter: "Green" oder "Red", optional ";<threshold>" (z.B. "Green;0.85")
    /// </summary>
    public sealed class SegmentWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 4) return 0d;

            double value = ToDouble(values[0]);
            double minimum = ToDouble(values[1]);
            double maximum = ToDouble(values[2]);
            double total = Math.Max(0d, ToDouble(values[3]));
            if (maximum <= minimum || total <= 0) return 0d;

            double frac = (value - minimum) / (maximum - minimum);
            frac = Math.Clamp(frac, 0d, 1d);

            // Parameter parsen: "Green" | "Red" | "Green;0.85"
            string p = (parameter as string) ?? "Green";
            string[] parts = p.Split(';');
            string which = parts[0].Trim();
            double threshold = 0.85; // 85% default
            if (parts.Length > 1 && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var th))
                threshold = Math.Clamp(th, 0d, 1d);

            if (which.Equals("Green", StringComparison.OrdinalIgnoreCase))
            {
                double greenFrac = Math.Min(frac, threshold);
                return greenFrac * total;
            }
            else
            {
                double redFrac = Math.Max(frac - threshold, 0d);
                return redFrac * total;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static double ToDouble(object o)
        {
            try { return System.Convert.ToDouble(o, CultureInfo.InvariantCulture); }
            catch { return 0d; }
        }
    }
}
