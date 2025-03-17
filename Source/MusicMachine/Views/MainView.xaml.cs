using MusicMachine.ViewModel;
using ReactiveUI;
using System;

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
