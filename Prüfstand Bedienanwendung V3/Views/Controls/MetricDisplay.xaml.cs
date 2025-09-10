using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Prüfstand_Bedienanwendung_V3.Data;
using Prüfstand_Bedienanwendung_V3.Services;
using Prüfstand_Bedienanwendung_V3.Views.Dialogs;

namespace Prüfstand_Bedienanwendung_V3.Views.Controls
{
    public partial class MetricDisplay : UserControl, INotifyPropertyChanged
    {
        // === Dependency Properties ===
        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(MetricDisplay), new PropertyMetadata(""));

        public string Unit { get => (string)GetValue(UnitProperty); set => SetValue(UnitProperty, value); }
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register(nameof(Unit), typeof(string), typeof(MetricDisplay), new PropertyMetadata(""));

        public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(MetricDisplay),
                new PropertyMetadata(0.0, OnValueChanged));

        public double ValueFontSize { get => (double)GetValue(ValueFontSizeProperty); set => SetValue(ValueFontSizeProperty, value); }
        public static readonly DependencyProperty ValueFontSizeProperty =
            DependencyProperty.Register(nameof(ValueFontSize), typeof(double), typeof(MetricDisplay), new PropertyMetadata(96.0));

        public bool ShowBar { get => (bool)GetValue(ShowBarProperty); set => SetValue(ShowBarProperty, value); }
        public static readonly DependencyProperty ShowBarProperty =
            DependencyProperty.Register(nameof(ShowBar), typeof(bool), typeof(MetricDisplay),
                new PropertyMetadata(true, OnShowBarChanged));

        public double BarHeight { get => (double)GetValue(BarHeightProperty); set => SetValue(BarHeightProperty, value); }
        public static readonly DependencyProperty BarHeightProperty =
            DependencyProperty.Register(nameof(BarHeight), typeof(double), typeof(MetricDisplay), new PropertyMetadata(12.0));

        public double Min { get => (double)GetValue(MinProperty); set => SetValue(MinProperty, value); }
        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register(nameof(Min), typeof(double), typeof(MetricDisplay),
                new PropertyMetadata(0.0, OnMinMaxChanged));

        public double Max { get => (double)GetValue(MaxProperty); set => SetValue(MaxProperty, value); }
        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register(nameof(Max), typeof(double), typeof(MetricDisplay),
                new PropertyMetadata(100.0, OnMinMaxChanged));

        public bool ShowScale { get => (bool)GetValue(ShowScaleProperty); set => SetValue(ShowScaleProperty, value); }
        public static readonly DependencyProperty ShowScaleProperty =
            DependencyProperty.Register(nameof(ShowScale), typeof(bool), typeof(MetricDisplay),
                new PropertyMetadata(false, OnShowScaleChanged));

        public double ScaleHeight { get => (double)GetValue(ScaleHeightProperty); set => SetValue(ScaleHeightProperty, value); }
        public static readonly DependencyProperty ScaleHeightProperty =
            DependencyProperty.Register(nameof(ScaleHeight), typeof(double), typeof(MetricDisplay),
                new PropertyMetadata(26.0, OnScaleVisualChanged));

        public string ScaleLabelFormat { get => (string)GetValue(ScaleLabelFormatProperty); set => SetValue(ScaleLabelFormatProperty, value); }
        public static readonly DependencyProperty ScaleLabelFormatProperty =
            DependencyProperty.Register(nameof(ScaleLabelFormat), typeof(string), typeof(MetricDisplay),
                new PropertyMetadata("N0", OnScaleVisualChanged));

        public string? SettingKey { get => (string?)GetValue(SettingKeyProperty); set => SetValue(SettingKeyProperty, value); }
        public static readonly DependencyProperty SettingKeyProperty =
            DependencyProperty.Register(nameof(SettingKey), typeof(string), typeof(MetricDisplay), new PropertyMetadata(null));

        // === INotifyPropertyChanged ===
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        public MetricDisplay()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            SizeChanged += (_, __) => { UpdateBarIndicator(); RedrawScale(); };
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (BarHost is UIElement bar) bar.Visibility = ShowBar ? Visibility.Visible : Visibility.Collapsed;
            if (ScaleHost is UIElement scl) scl.Visibility = ShowScale ? Visibility.Visible : Visibility.Collapsed;

            PART_Track.SizeChanged += (_, __) => UpdateBarIndicator();
            ScaleCanvas.SizeChanged += (_, __) => RedrawScale();

            UpdateBarIndicator();
            RedrawScale();
            TryApplySavedSettings();
        }

        private void OnTileMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var dlg = new MetricSettingsDialog(this) { Owner = Window.GetWindow(this) };
                dlg.ShowDialog();
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((MetricDisplay)d).UpdateBarIndicator();

        private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (MetricDisplay)d;
            self.UpdateBarIndicator();
            self.RedrawScale();
        }

        private static void OnShowBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (MetricDisplay)d;
            self.BarHost.Visibility = self.ShowBar ? Visibility.Visible : Visibility.Collapsed;
            self.UpdateBarIndicator();
        }

        private static void OnShowScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (MetricDisplay)d;
            self.ScaleHost.Visibility = self.ShowScale ? Visibility.Visible : Visibility.Collapsed;
            self.RedrawScale();
        }

        private static void OnScaleVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((MetricDisplay)d).RedrawScale();

        private void UpdateBarIndicator()
        {
            if (!ShowBar || PART_Track.ActualWidth <= 0)
            {
                PART_Indicator.Width = 0;
                return;
            }

            double range = Max - Min;
            if (range <= 0 || double.IsNaN(Value)) { PART_Indicator.Width = 0; return; }

            double t = (Value - Min) / range;
            t = Math.Max(0, Math.Min(1, t));
            PART_Indicator.Width = PART_Track.ActualWidth * t;
        }

        private void RedrawScale()
        {
            if (!ShowScale) { ScaleCanvas.Children.Clear(); return; }
            double w = ScaleCanvas.ActualWidth; double h = ScaleCanvas.ActualHeight;
            ScaleCanvas.Children.Clear();
            if (w <= 1 || h <= 1) return;

            var tickBrush = new SolidColorBrush(Color.FromRgb(106, 106, 106));
            var textBrush = new SolidColorBrush(Color.FromRgb(158, 158, 158));
            double tickHeight = Math.Max(8, h * 0.6);
            double y0 = 0;

            void DrawTick(double x)
            {
                var line = new Rectangle { Width = 2, Height = tickHeight, Fill = tickBrush };
                Canvas.SetLeft(line, x - 1); Canvas.SetTop(line, y0); ScaleCanvas.Children.Add(line);
            }

            DrawTick(0); DrawTick(w * 0.5); DrawTick(w);

            TextBlock MakeLabel(string txt) => new TextBlock { Text = txt, Foreground = textBrush, FontSize = 12 };

            var minText = MakeLabel(Min.ToString(ScaleLabelFormat, CultureInfo.CurrentCulture));
            var midText = MakeLabel(((Min + Max) * 0.5).ToString(ScaleLabelFormat, CultureInfo.CurrentCulture));
            var maxText = MakeLabel(Max.ToString(ScaleLabelFormat, CultureInfo.CurrentCulture));

            double labelY = tickHeight + 2;
            void Place(UIElement e, double x, double y) { Canvas.SetLeft(e, x); Canvas.SetTop(e, y); ScaleCanvas.Children.Add(e); }

            minText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            midText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            maxText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            Place(minText, 0 - minText.DesiredSize.Width * 0.5, labelY);
            Place(midText, w * 0.5 - midText.DesiredSize.Width * 0.5, labelY);
            Place(maxText, w - maxText.DesiredSize.Width * 0.5, labelY);
        }

        public void TryApplySavedSettings()
        {
            if (string.IsNullOrWhiteSpace(SettingKey)) return;
            var s = TileSettingsService.TryGet(SettingKey);
            if (s.HasValue)
            {
                Min = s.Value.Min; Max = s.Value.Max;
                if (!string.IsNullOrWhiteSpace(s.Value.MetricKey))
                {
                    var b = new Binding(s.Value.MetricKey!)
                    {
                        Source = GlobalData.Instance,
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                    BindingOperations.SetBinding(this, ValueProperty, b);

                    var def = MetricCatalog.Find(s.Value.MetricKey!);
                    if (def != null) { Title = def.DisplayName; Unit = def.Unit; }
                    UpdateBarIndicator(); RedrawScale();
                }
            }
        }
    }
}
