using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Prüfstand_Bedienanwendung_V3.Data
{
    public sealed class GlobalData : INotifyPropertyChanged
    {
        private static readonly Lazy<GlobalData> _lazy = new(() => new GlobalData());
        public static GlobalData Instance => _lazy.Value;

        private GlobalData() { }

        private double _speedKmh;
        public double SpeedKmh { get => _speedKmh; set { if (_speedKmh != value) { _speedKmh = value; OnPropertyChanged(); } } }

        private double _rpm;
        public double Rpm { get => _rpm; set { if (_rpm != value) { _rpm = value; OnPropertyChanged(); } } }

        private double _torqueNm;
        public double TorqueNm { get => _torqueNm; set { if (_torqueNm != value) { _torqueNm = value; OnPropertyChanged(); } } }

        private double _powerPs;
        public double PowerPS { get => _powerPs; set { if (_powerPs != value) { _powerPs = value; OnPropertyChanged(); } } }

        private double _dinFactorPct;
        public double DINFactorPct { get => _dinFactorPct; set { if (_dinFactorPct != value) { _dinFactorPct = value; OnPropertyChanged(); } } }

        private double _temperatureC;
        public double TemperatureC { get => _temperatureC; set { if (_temperatureC != value) { _temperatureC = value; OnPropertyChanged(); } } }

        private double _pressurehPa;
        public double PressurehPa { get => _pressurehPa; set { if (_pressurehPa != value) { _pressurehPa = value; OnPropertyChanged(); } } }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
