using System.Windows.Controls;

namespace Prüfstand_Bedienanwendung_V3.Views
{
    public partial class MenuPage : Page
    {
        public MenuPage()
        {
            InitializeComponent();
        }

        private void LiveAnzeige_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // zur Live Anzeige navigieren
            this.NavigationService?.Navigate(new LiveAnzeigePage());
        }
    }
}
