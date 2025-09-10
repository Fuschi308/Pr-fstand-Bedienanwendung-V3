using System;
using System.Globalization;
using System.Windows.Data;

namespace Prüfstand_Bedienanwendung_V3.Converters
{
    /// <summary>
    /// values[0]=double (Value), values[1]=string (Format).
    /// </summary>
    public sealed class FormatDoubleMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double val = 0.0;
            string fmt = "N0";

            if (values != null)
            {
                if (values.Length > 0 && values[0] != null)
                {
                    try { val = System.Convert.ToDouble(values[0], CultureInfo.InvariantCulture); }
                    catch { val = 0.0; }
                }
                if (values.Length > 1 && values[1] is string s && !string.IsNullOrWhiteSpace(s))
                {
                    fmt = s;
                }
            }

            if (double.IsNaN(val) || double.IsInfinity(val)) return "—";
            try { return val.ToString(fmt, CultureInfo.CurrentCulture); }
            catch { return val.ToString("N0", CultureInfo.CurrentCulture); }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
