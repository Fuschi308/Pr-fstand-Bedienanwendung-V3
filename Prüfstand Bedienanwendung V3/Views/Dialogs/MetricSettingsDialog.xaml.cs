using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using Prüfstand_Bedienanwendung_V3.Data;
using Prüfstand_Bedienanwendung_V3.Services;
using Prüfstand_Bedienanwendung_V3.Views.Controls;

namespace Prüfstand_Bedienanwendung_V3.Views.Dialogs
{
    public partial class MetricSettingsDialog : Window, INotifyPropertyChanged
    {
        private readonly MetricDisplay _tile;

        public string MinText { get => _minText; set { _minText = value; OnPropertyChanged(); } }
        public string MaxText { get => _maxText; set { _maxText = value; OnPropertyChanged(); } }
        public MetricDefinition[] Metrics => MetricCatalog.All;

        public string SelectedMetricKey
        {
            get => _selectedMetricKey;
            set { if (_selectedMetricKey != value) { _selectedMetricKey = value; OnPropertyChanged(); } }
        }

        private string _minText = "0";
        private string _maxText = "100";
        private string _selectedMetricKey = MetricCatalog.Default.Key;

        public MetricSettingsDialog(MetricDisplay tile)
        {
            InitializeComponent();
            _tile = tile ?? throw new ArgumentNullException(nameof(tile));
            DataContext = this;

            // Vorbelegung aus Kachel
            MinText = _tile.Min.ToString(CultureInfo.CurrentCulture);
            MaxText = _tile.Max.ToString(CultureInfo.CurrentCulture);

            // Geladene Settings
            if (!string.IsNullOrWhiteSpace(_tile.SettingKey))
            {
                var s = TileSettingsService.TryGet(_tile.SettingKey);
                if (s.HasValue)
                {
                    MinText = s.Value.Min.ToString(CultureInfo.CurrentCulture);
                    MaxText = s.Value.Max.ToString(CultureInfo.CurrentCulture);
                    if (!string.IsNullOrWhiteSpace(s.Value.MetricKey))
                        SelectedMetricKey = s.Value.MetricKey!;
                }
            }
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(MinText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var newMin))
            {
                MessageBox.Show(this, "Ungültiger Min-Wert.", "Eingabe", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!double.TryParse(MaxText, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var newMax))
            {
                MessageBox.Show(this, "Ungültiger Max-Wert.", "Eingabe", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (newMax <= newMin)
            {
                MessageBox.Show(this, "Max muss größer als Min sein.", "Eingabe", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Auf Kachel anwenden
            _tile.Min = newMin;
            _tile.Max = newMax;

            // Binding auf GLOBAL data (Rohdaten)
            var b = new Binding(SelectedMetricKey)
            {
                Source = GlobalData.Instance,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(_tile, MetricDisplay.ValueProperty, b);

            // Titel/Einheit übernehmen
            var def = MetricCatalog.Find(SelectedMetricKey);
            if (def is not null)
            {
                _tile.Title = def.DisplayName;
                _tile.Unit = def.Unit;
            }

            // Persistieren
            if (!string.IsNullOrWhiteSpace(_tile.SettingKey))
            {
                try { TileSettingsService.Save(_tile.SettingKey, newMin, newMax, SelectedMetricKey); }
                catch (Exception ex) { MessageBox.Show(this, "Speichern fehlgeschlagen: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error); }
            }

            DialogResult = true;
            Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
