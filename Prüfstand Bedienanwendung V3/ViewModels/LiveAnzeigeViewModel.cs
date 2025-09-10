using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using Prüfstand_Bedienanwendung_V3.Services.Simulation;
using Prüfstand_Bedienanwendung_V3.Utils;

namespace Prüfstand_Bedienanwendung_V3.ViewModels
{
    public class LiveAnzeigeViewModel : INotifyPropertyChanged
    {
        // Messwerte
        private double _speedKmh, _rpm, _torqueNm, _powerPS, _dinFactorPct, _temperatureC, _pressurehPa;

        public double SpeedKmh { get => _speedKmh; set { _speedKmh = value; OnPropertyChanged(); } }
        public double Rpm { get => _rpm; set { _rpm = value; OnPropertyChanged(); } }
        public double TorqueNm { get => _torqueNm; set { _torqueNm = value; OnPropertyChanged(); } }
        public double PowerPS { get => _powerPS; set { _powerPS = value; OnPropertyChanged(); } }
        public double DINFactorPct { get => _dinFactorPct; set { _dinFactorPct = value; OnPropertyChanged(); } }
        public double TemperatureC { get => _temperatureC; set { _temperatureC = value; OnPropertyChanged(); } }
        public double PressurehPa { get => _pressurehPa; set { _pressurehPa = value; OnPropertyChanged(); } }

        // Simulation
        private readonly LiveDataSimulator _sim;
        private bool _isSimulating;
        public bool IsSimulating { get => _isSimulating; private set { _isSimulating = value; OnPropertyChanged(); } }

        public ICommand ToggleSimulationCommand { get; }

        public LiveAnzeigeViewModel()
        {
            _sim = new LiveDataSimulator(Dispatcher.CurrentDispatcher);
            ToggleSimulationCommand = new RelayCommand(_ => ToggleSimulation());
        }

        private void ToggleSimulation()
        {
            if (_sim.IsRunning) { _sim.Stop(); IsSimulating = false; }
            else { _sim.Start(this); IsSimulating = true; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
