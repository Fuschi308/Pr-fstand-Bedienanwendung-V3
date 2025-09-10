// Datei: Prüfstand Bedienanwendung V3/Prüfstand Bedienanwendung V3/Services/Simulation/LiveDataSimulator.cs
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Prüfstand_Bedienanwendung_V3.ViewModels;
using Prüfstand_Bedienanwendung_V3.Data;

namespace Prüfstand_Bedienanwendung_V3.Services.Simulation
{
    /// <summary>
    /// Dyno-Simulation mit getrennten Raten:
    /// - Physik-Tick: klein (10 ms) für sauberen Kurvenverlauf
    /// - UI-Update: langsamer (50 ms ≈ 20 Hz) für ruhige Anzeige
    ///
    /// Alle ausgegebenen Werte werden auf EXAKT 2 Dezimalstellen gerundet.
    /// (Hinweis: Für eine Anzeige immer mit 2 Nachkommastellen in der UI ggf. StringFormat="F2" verwenden.)
    /// </summary>
    public class LiveDataSimulator
    {
        private readonly Dispatcher _dispatcher;
        private CancellationTokenSource? _cts;

        // --- Raten ---
        private const int PhysicsTickMs = 10;  // Simulationsschritt (intern)
        private const int UiUpdateMs = 50;  // Anzeige-Update (20 Hz)

        // --- Fahrzyklus-Parameter (verlangsamt) ---
        private const double IdleRpm = 1500.0;
        private const double RedlineRpm = 12000.0;
        private const double PeakRpm = 7500.0;      // typische Peak-Drehzahl fürs Drehmoment
        private const double VmaxAtRedlineKmh = 300.0;       // rpm -> km/h (vereinfachte Linearität)

        // Zeitabschnitte in Sekunden
        private const double T_Idle1 = 3.0;
        private const double T_Ramp = 25.0;  // langsamer Sweep
        private const double T_Hold = 3.0;
        private const double T_Coast = 12.0;
        private const double T_Idle2 = 3.0;

        private const double CycleT = T_Idle1 + T_Ramp + T_Hold + T_Coast + T_Idle2;

        // --- Drehmomentkennlinie (vereinfachtes Modell) ---
        private const double TorquePeakNm = 120.0;  // max. Drehmoment
        private const double TorqueIdleNm = 30.0;   // Baseline bei Idle
        private const double TopDropFrac = 0.45;   // relativer Abfall nach Peak
        private const double DropExponent = 1.2;    // Form des Abfalls > Peak (1..2)

        public LiveDataSimulator(Dispatcher dispatcher) => _dispatcher = dispatcher;

        public bool IsRunning => _cts is { IsCancellationRequested: false };

        public void Start(LiveAnzeigeViewModel vm)
        {
            if (IsRunning) return;
            _cts = new CancellationTokenSource();
            _ = RunLoopAsync(vm, _cts.Token);
        }

        public void Stop()
        {
            try { _cts?.Cancel(); } catch { /* ignore */ }
            _cts = null;
        }

        private async Task RunLoopAsync(LiveAnzeigeViewModel vm, CancellationToken ct)
        {
            // Startwerte setzen (einmalig, bereits gerundet)
            await _dispatcher.InvokeAsync(() =>
            {
                vm.SpeedKmh = Round2(0);
                vm.Rpm = Round2(0);
                vm.TorqueNm = Round2(0);
                vm.PowerPS = Round2(0);
                vm.DINFactorPct = Round2(100);
                vm.TemperatureC = Round2(22);
                vm.PressurehPa = Round2(1012);

                GlobalData.Instance.SpeedKmh = vm.SpeedKmh;
                GlobalData.Instance.Rpm = vm.Rpm;
                GlobalData.Instance.TorqueNm = vm.TorqueNm;
                GlobalData.Instance.PowerPS = vm.PowerPS;
                GlobalData.Instance.DINFactorPct = vm.DINFactorPct;
                GlobalData.Instance.TemperatureC = vm.TemperatureC;
                GlobalData.Instance.PressurehPa = vm.PressurehPa;
            });

            var sw = Stopwatch.StartNew();
            var last = sw.Elapsed;
            var lastUi = last;
            double tCycle = 0.0;

            while (!ct.IsCancellationRequested)
            {
                var now = sw.Elapsed;
                var dt = (now - last).TotalSeconds;
                last = now;

                // dt begrenzen (z.B. bei Debugger-Break)
                if (!double.IsFinite(dt) || dt <= 0) dt = PhysicsTickMs / 1000.0;
                if (dt > 0.2) dt = PhysicsTickMs / 1000.0;

                // Zykluszeit fortschreiben
                tCycle += dt;
                if (tCycle >= CycleT) tCycle -= CycleT;

                // --- Simulation berechnen (Physik-Takt) ---
                double rpm = RpmFromCycle(tCycle);
                double torque = TorqueFromRpm(rpm);
                double powerPS = torque * rpm / 7023.0;
                double speed = (rpm / RedlineRpm) * VmaxAtRedlineKmh;

                // Umgebungswerte (langsame Sinus-Modulation)
                double tAbs = now.TotalSeconds;
                double dinPct = 100.0 + 1.5 * Math.Sin(2.0 * Math.PI * 0.01 * tAbs + 0.7);   // +/-1.5 %
                double tempC = 21.0 + 3.0 * Math.Sin(2.0 * Math.PI * 0.006 * tAbs + 2.1);  // 18..24 °C
                double pressHpa = 1008.0 + 6.0 * Math.Sin(2.0 * Math.PI * 0.004 * tAbs + 3.2); // 1002..1014 hPa

                // Leichter Jitter im Hold-Bereich (Begrenzer-Anmutung)
                if (tCycle >= (T_Idle1 + T_Ramp) && tCycle < (T_Idle1 + T_Ramp + T_Hold))
                {
                    rpm += 50.0 * Math.Sin(2.0 * Math.PI * 6.0 * tAbs);
                    powerPS *= 1.002 + 0.002 * Math.Sin(2.0 * Math.PI * 5.0 * tAbs + 0.5);
                }

                // --- UI-Update nur alle UiUpdateMs (mit Rundung auf 2 Dezimalstellen) ---
                if ((now - lastUi).TotalMilliseconds >= UiUpdateMs)
                {
                    lastUi = now;

                    // EINMAL runden und diese Werte überall verwenden
                    double rSpeed = Round2(speed);
                    double rRpm = Round2(rpm);
                    double rTq = Round2(torque);
                    double rPwr = Round2(powerPS);
                    double rDin = Round2(dinPct);
                    double rTemp = Round2(tempC);
                    double rPres = Round2(pressHpa);

                    try
                    {
                        await _dispatcher.InvokeAsync(() =>
                        {
                            vm.SpeedKmh = rSpeed;
                            vm.Rpm = rRpm;
                            vm.TorqueNm = rTq;
                            vm.PowerPS = rPwr;
                            vm.DINFactorPct = rDin;
                            vm.TemperatureC = rTemp;
                            vm.PressurehPa = rPres;

                            GlobalData.Instance.SpeedKmh = rSpeed;
                            GlobalData.Instance.Rpm = rRpm;
                            GlobalData.Instance.TorqueNm = rTq;
                            GlobalData.Instance.PowerPS = rPwr;
                            GlobalData.Instance.DINFactorPct = rDin;
                            GlobalData.Instance.TemperatureC = rTemp;
                            GlobalData.Instance.PressurehPa = rPres;
                        }, DispatcherPriority.Render);
                    }
                    catch (TaskCanceledException)
                    {
                        // Ignore on shutdown
                    }
                }

                try { await Task.Delay(PhysicsTickMs, ct).ConfigureAwait(false); }
                catch (TaskCanceledException) { /* stop */ }
            }
        }

        /// <summary>Kommerzielle Rundung auf 2 Dezimalstellen (0,005 -> 0,01).</summary>
        private static double Round2(double v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        /// <summary>
        /// RPM über Zyklus (verlangsamt):
        /// Idle1 (konstant) -> linearer Ramp-Up -> Hold -> linearer Coast-Down -> Idle2 (konstant)
        /// </summary>
        private static double RpmFromCycle(double tc)
        {
            double t = tc;

            if (t < T_Idle1) return IdleRpm;
            t -= T_Idle1;

            if (t < T_Ramp)
            {
                double u = t / T_Ramp;     // 0..1
                double s = SmoothStep(u);  // weicher Verlauf
                return Lerp(IdleRpm, RedlineRpm, s);
            }
            t -= T_Ramp;

            if (t < T_Hold)
            {
                // kurzer Halt nahe Begrenzer (minimal „atmend“)
                return RedlineRpm - 60.0 + 60.0 * 0.5 * (1.0 + Math.Sin(2.0 * Math.PI * 1.6 * t));
            }
            t -= T_Hold;

            if (t < T_Coast)
            {
                double u = t / T_Coast;    // 0..1
                double s = SmoothStep(u);
                return Lerp(RedlineRpm, IdleRpm, s);
            }

            // Idle2
            return IdleRpm;
        }

        /// <summary>
        /// Vereinfachte Drehmomentkurve:
        /// - steigt von Idle zu Peak (SmoothStep)
        /// - fällt nach Peak weich ab (Potenz)
        /// </summary>
        private static double TorqueFromRpm(double rpm)
        {
            double r = Math.Max(IdleRpm, Math.Min(rpm, RedlineRpm));

            if (r <= PeakRpm)
            {
                double u = (r - IdleRpm) / Math.Max(1.0, (PeakRpm - IdleRpm)); // 0..1
                double s = SmoothStep(Clamp01(u));
                return Lerp(TorqueIdleNm, TorquePeakNm, s);
            }
            else
            {
                // Abfall nach Peak
                double u = (r - PeakRpm) / Math.Max(1.0, (RedlineRpm - PeakRpm)); // 0..1
                u = Clamp01(u);
                double fall = Math.Pow(u, DropExponent); // 0..1
                double target = TorquePeakNm * (1.0 - TopDropFrac * fall);
                return Math.Max(target, TorqueIdleNm * 0.8); // nicht unter Baseline fallen lassen
            }
        }

        // --- Hilfsfunktionen ---
        private static double Lerp(double a, double b, double t) => a + (b - a) * t;
        private static double SmoothStep(double u) { u = Clamp01(u); return u * u * (3.0 - 2.0 * u); }
        private static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);
    }
}
