using Microsoft.Win32;
using MusicMachine.Model;
using MusicMachine.ViewModel.SequenzerVM;
using MusicMachine.Views.SequenzerVM.SequenzerCanvasElements;
using ReactiveUI;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using static MusicMachine.Views.SequenzerVM.SequenzerCanvasElements.RectangleNotes;

namespace MusicMachine.Views.SequenzerVM
{
    /// <summary>
    /// Interaktionslogik für SequenzerView.xaml
    /// </summary>
    public partial class SequenzerView : ReactiveUserControl<SequenzerViewModel>
    {
        public SequenzerView()
        {
            InitializeComponent();

            

            this.WhenActivated(disposableRegistration =>
            {
                this.ViewModel = this.DataContext as SequenzerViewModel;

                Observable.FromEventPattern<int>(this.sequenzerCanvas1.PlayLine, nameof(PlayLine.MouseMoveSamplePositionEvent))
                    .Select(x => x.EventArgs)
                    .InvokeCommand(ViewModel.MouseMoveSamplePosition)
                    .DisposeWith(disposableRegistration);

                Observable.FromEventPattern<MouseNoteEventArgs>(this.sequenzerCanvas1.RectangleNotes, nameof(RectangleNotes.CreateNoteEvent))
                    .Select(x => x.EventArgs)
                    .InvokeCommand(ViewModel.CreateNoteCommand)
                    .DisposeWith(disposableRegistration);

                this.sequenzerCanvas1.Events().PreviewMouseDown.Select(x => Unit.Default).InvokeCommand(this.ViewModel.MouseClickCanvas);

                this
                .ViewModel
                .OpenFileDialog
                .RegisterOpenFileDialog()
                .DisposeWith(disposableRegistration);

                this
                .ViewModel
                .SaveFileDialog
                .RegisterSaveFileDialog()
                .DisposeWith(disposableRegistration);

                //......Piano Canvas

                Observable.FromEventPattern<int>(this.pianoCanvas1, nameof(PianoCanvas.PianoKeyDownEvent))
                    .Select(x => x.EventArgs)
                    .InvokeCommand(ViewModel.PianoViewModel.PlayToneCommand)
                    .DisposeWith(disposableRegistration);

                Observable.FromEventPattern<int>(this.pianoCanvas1, nameof(PianoCanvas.PianoKeyUpEvent))
                    .Select(x => x.EventArgs)
                    .InvokeCommand(ViewModel.PianoViewModel.ReleaseToneCommand)
                    .DisposeWith(disposableRegistration);
            });
        }
    }
}
