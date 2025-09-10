using System.Windows.Controls;
using System.Windows.Media;

namespace Prüfstand_Bedienanwendung_V3.Views.Controls
{
    public partial class PlcStatusControl : UserControl
    {
        public PlcStatusControl()
        {
            InitializeComponent();

            // Platzhalter – später SPS-Abfrage über PlcService
            bool plcConnected = false;

            if (plcConnected)
            {
                StatusText.Text = "SPS: Verbunden";
                StatusText.Foreground = Brushes.LimeGreen;
            }
            else
            {
                StatusText.Text = "SPS: Getrennt";
                StatusText.Foreground = Brushes.Red;
            }
        }
    }
}
