using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Prüfstand_Bedienanwendung_V3.Services;
using Prüfstand_Bedienanwendung_V3.Views;

namespace Prüfstand_Bedienanwendung_V3
{
    public partial class MainWindow : Window
    {
        private readonly WindowManager _windowManager;

        public MainWindow()
        {
            InitializeComponent();

            _windowManager = new WindowManager(this, TitleBar);

            // Seite laden
            MainFrame.Navigate(new StartPage(MainFrame));

            // Beim Start: echtes Vollbild (verdeckt Taskleiste)
            _windowManager.GoFullScreen();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
            => _windowManager.Minimize();

        private void Maximize_Click(object sender, RoutedEventArgs e)
            => _windowManager.ToggleMaximize();

        private void Close_Click(object sender, RoutedEventArgs e)
            => _windowManager.Close();

        private void Window_KeyDown(object sender, KeyEventArgs e)
            => _windowManager.HandleKeyDown(e);

        // Entferne diese Methode, wenn du NICHT verschieben willst
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !_windowManager.IsFullScreen)
            {
                DragMove();
            }
        }
    }
}
