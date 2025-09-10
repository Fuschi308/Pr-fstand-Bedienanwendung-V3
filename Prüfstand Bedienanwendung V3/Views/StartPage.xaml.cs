using System.Windows;
using System.Windows.Controls;

namespace Prüfstand_Bedienanwendung_V3.Views
{
    public partial class StartPage : Page
    {
        private Frame _frame;

        public StartPage(Frame frame)
        {
            InitializeComponent();
            _frame = frame;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            _frame.Navigate(new MenuPage());
        }
    }
}
