using System.Windows.Controls;
using Prüfstand_Bedienanwendung_V3.ViewModels;

namespace Prüfstand_Bedienanwendung_V3.Views
{
    public partial class LiveAnzeigePage : Page
    {
        public LiveAnzeigePage()
        {
            InitializeComponent();
            DataContext = new LiveAnzeigeViewModel();
        }
    }
}
