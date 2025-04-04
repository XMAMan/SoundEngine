using SoundEngineTest.ViewModel;
using System.Text;
using System.Windows;

namespace SoundEngineTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();

            double l1 = (1 + Math.Sqrt(5)) / 2;
            double l2 = (1 - Math.Sqrt(5)) / 2;
            double l = 1 / Math.Sqrt(5);
            StringBuilder str = new StringBuilder();
            for (int i=0;i<100;i++)
            {
                double fn = l * (Math.Pow(l1, i) - Math.Pow(l2, i));
                str.AppendLine(i + ": " + fn);
                str.AppendLine(i + ": " + Math.Pow(l1, i));
                str.AppendLine(i + ": " + Math.Pow(l2, i));
            }
            string result = str.ToString();


            this.DataContext = viewModel;

            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            this.viewModel.Dispose();
        }
    }
}
