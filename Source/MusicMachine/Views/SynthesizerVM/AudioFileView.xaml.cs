using MusicMachine.Model;
using MusicMachine.ViewModel.SynthesizerVM;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicMachine.Views.SynthesizerVM
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
