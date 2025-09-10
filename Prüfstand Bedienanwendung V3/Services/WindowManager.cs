using System.Windows;
using System.Windows.Input;

namespace Prüfstand_Bedienanwendung_V3.Services
{
    public class WindowManager
    {
        private readonly Window _window;
        private readonly FrameworkElement? _titleBar;

        public bool IsFullScreen { get; private set; }

        public WindowManager(Window window, FrameworkElement? titleBar = null)
        {
            _window = window;
            _titleBar = titleBar;
        }

        public void Minimize() => _window.WindowState = WindowState.Minimized;

        public void ToggleMaximize()
        {
            if (IsFullScreen) return; // im echten Vollbild kein Max/Normal

            if (_window.WindowState == WindowState.Maximized)
                _window.WindowState = WindowState.Normal;
            else
                _window.WindowState = WindowState.Maximized;
        }

        public void Close() => _window.Close();

        public void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.F11) ToggleFullScreen();
            if (e.Key == Key.Escape && IsFullScreen) ExitFullScreen();
        }

        public void ToggleFullScreen()
        {
            if (IsFullScreen) ExitFullScreen();
            else GoFullScreen();
        }

        public void GoFullScreen()
        {
            // echtes Vollbild: Taskleiste verdecken
            _window.WindowStyle = WindowStyle.None;
            _window.ResizeMode = ResizeMode.NoResize;
            _window.Topmost = true;

            _window.Left = 0;
            _window.Top = 0;
            _window.Width = SystemParameters.PrimaryScreenWidth;
            _window.Height = SystemParameters.PrimaryScreenHeight;

            if (_titleBar != null) _titleBar.Visibility = Visibility.Collapsed;

            IsFullScreen = true;
        }

        public void ExitFullScreen()
        {
            _window.Topmost = false;
            _window.WindowStyle = WindowStyle.None;     // weiter ohne Systemleiste
            _window.ResizeMode = ResizeMode.CanResize;
            _window.WindowState = WindowState.Normal;

            // definierte Standardgröße im Fenstermodus
            _window.Width = 1280;
            _window.Height = 800;

            if (_titleBar != null) _titleBar.Visibility = Visibility.Visible;

            IsFullScreen = false;
        }
    }
}
