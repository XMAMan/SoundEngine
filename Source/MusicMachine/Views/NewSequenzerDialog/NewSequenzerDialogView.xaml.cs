using System.Windows;

namespace MusicMachine.Views.NewSequenzerDialog
{
    /// <summary>
    /// Interaktionslogik für NewSequenzerDialogView.xaml
    /// </summary>
    public partial class NewSequenzerDialogView : Window
    {
        public NewSequenzerDialogView()
        {
            InitializeComponent();

            this.OkButton.Click += OkButton_Click;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
