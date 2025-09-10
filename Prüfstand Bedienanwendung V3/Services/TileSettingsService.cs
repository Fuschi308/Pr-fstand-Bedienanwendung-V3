using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Prüfstand_Bedienanwendung_V3.Services
{
    // NOTE: struct, damit TryGet(...) als MetricTileSettings? verwendet werden kann (.HasValue/.Value)
    public struct MetricTileSettings
    {
        [JsonPropertyName("min")] public double Min { get; set; }
        [JsonPropertyName("max")] public double Max { get; set; }
        [JsonPropertyName("metric")] public string? MetricKey { get; set; }
    }

    internal sealed class SettingsStore
    {
        [JsonPropertyName("tiles")]
        public Dictionary<string, MetricTileSettings> Tiles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public static class TileSettingsService
    {
        private static readonly object _sync = new();

        private static readonly string _dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Pruefstand_Bedienanwendung_V3");

        private static readonly string _file = Path.Combine(_dir, "tile_settings.json");

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        private static SettingsStore _cache = new();
        private static bool _loaded = false;

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            lock (_sync)
            {
                if (_loaded) return;
                try
                {
                    Directory.CreateDirectory(_dir);
                    if (File.Exists(_file))
                    {
                        var json = File.ReadAllText(_file);
                        _cache = JsonSerializer.Deserialize<SettingsStore>(json, _jsonOptions) ?? new SettingsStore();
                    }
                }
                catch
                {
                    _cache = new SettingsStore();
                }
                _loaded = true;
            }
        }

        private static void SaveNow()
        {
            try
            {
                Directory.CreateDirectory(_dir);
                var json = JsonSerializer.Serialize(_cache, _jsonOptions);
                File.WriteAllText(_file, json);
            }
            catch
            {
                // bewusst geschluckt – UI soll nicht abstürzen
            }
        }

        public static MetricTileSettings? TryGet(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            EnsureLoaded();
            lock (_sync)
            {
                return _cache.Tiles.TryGetValue(key, out var val) ? val : null;
            }
        }

        public static void Save(string key, double min, double max, string? metricKey = null)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            EnsureLoaded();
            lock (_sync)
            {
                _cache.Tiles[key] = new MetricTileSettings
                {
                    Min = min,
                    Max = max,
                    MetricKey = metricKey
                };
                SaveNow();
            }
        }
    }
}
