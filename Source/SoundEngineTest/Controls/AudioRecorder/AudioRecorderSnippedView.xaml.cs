using ReactiveUI;
using SoundEngineTest.Controls.AudioRecorder;
using System.Reactive.Disposables;

namespace SoundEngineTest
{
    /// <summary>
    /// Interaktionslogik für AudioRecorderSnippedView.xaml
    /// </summary>
    public partial class AudioRecorderSnippedView : ReactiveUserControl<AudioRecorderSnippedViewModel>
    {
        public AudioRecorderSnippedView()
        {
            InitializeComponent();

            this.WhenActivated(disposableRegistration =>
            {
                this.ViewModel = this.DataContext as AudioRecorderSnippedViewModel;

                this
                   .ViewModel
                   .SaveFileDialog
                   .RegisterSaveFileDialog()
                   .DisposeWith(disposableRegistration);
            });
        }
    }
}
