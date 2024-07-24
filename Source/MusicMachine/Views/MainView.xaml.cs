using MusicMachine.ViewModel;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicMachine.Views
{
    /// <summary>
    /// Interaktionslogik für MainView.xaml
    /// </summary>
    public partial class MainView : ReactiveWindow<MainViewModel>
    {
        private MainViewModel model = new MainViewModel();
        public MainView()
        {
            InitializeComponent();

            //Wird per Xaml gemacht
            //Uri iconUri = new Uri("pack://application:,,,/MusicMachine;component/Resources/Note.png");
            //this.Icon = BitmapFrame.Create(iconUri);

            this.DataContext = model;

            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (this.model is IDisposable)
            {
                (this.model as IDisposable).Dispose();
            }
        }
    }
}
