using ReactiveUI;

namespace MusicMachine.Controls.SynthesizerElements.AudioFileControl
{
    /// <summary>
    /// Interaktionslogik für AudioFileView.xaml
    /// </summary>
    public partial class AudioFileView : ReactiveUserControl<AudioFileViewModel>
    {
        public AudioFileView()
        {
            InitializeComponent();

            this.WhenActivated(disposableRegistration =>
            {
                this.ViewModel = this.DataContext as AudioFileViewModel;

                /*this
                .ViewModel
                .OpenFileDialog
                .RegisterOpenFileDialog()
                .DisposeWith(disposableRegistration);*/
            });
        }
    }
}
