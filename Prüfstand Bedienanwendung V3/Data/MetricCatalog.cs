using System;
using System.Linq;

namespace Prüfstand_Bedienanwendung_V3.Data
{
    public sealed record MetricDefinition(string Key, string DisplayName, string Unit, double DefaultMin, double DefaultMax);

    public static class MetricCatalog
    {
        public static readonly MetricDefinition[] All = new[]
        {
            new MetricDefinition("SpeedKmh",     "Geschwindigkeit", "km/h",  0,   300),
            new MetricDefinition("Rpm",          "Motordrehzahl",   "RPM",   0, 15000),
            new MetricDefinition("TorqueNm",     "Drehmoment",      "Nm",    0,  1000),
            new MetricDefinition("PowerPS",      "Leistung",        "PS",    0,   250),
            new MetricDefinition("DINFactorPct", "DIN Faktor",      "%",    90,   110),
            new MetricDefinition("TemperatureC", "Temperatur",      "°C",  -10,    80),
            new MetricDefinition("PressurehPa",  "Druck",           "hPa", 950,  1050),
        };

        public static MetricDefinition? Find(string key)
            => All.FirstOrDefault(m => m.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

        public static MetricDefinition Default => All[0];
    }
}
