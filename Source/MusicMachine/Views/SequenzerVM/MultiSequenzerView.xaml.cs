using Microsoft.Win32;
using MusicMachine.Model;
using MusicMachine.ViewModel.NewSequenzerDialog;
using MusicMachine.ViewModel.SequenzerVM;
using MusicMachine.Views.NewSequenzerDialog;
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

namespace MusicMachine.Views.SequenzerVM
{
    /// <summary>
    /// Interaktionslogik für MultiSequenzerView.xaml
    /// </summary>
    public partial class MultiSequenzerView : ReactiveUserControl<MultiSequenzerViewModel>
    {
        public MultiSequenzerView()
        {
            InitializeComponent();

            this.WhenActivated(disposableRegistration =>
            {
                this.ViewModel = this.DataContext as MultiSequenzerViewModel;

                var keyDown = Observable.FromEventPattern<KeyEventArgs>(Window.GetWindow(this), nameof(Window.KeyDown)) //Weg 1
                    .Select(x => x.EventArgs);

                this.ViewModel.SetKeyEventHandler(this.Events().KeyDown, this.Events().KeyUp);

                this.PlayTone.Events().PreviewMouseDown.Select(x => Unit.Default).InvokeCommand(this.ViewModel.PlayTestToneMouseDown);
                this.PlayTone.Events().PreviewMouseUp.Select(x => Unit.Default).InvokeCommand(this.ViewModel.PlayTestToneMouseUp);


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

                /*this
                    .ViewModel
                    .NewSequenzerDialog
                    .RegisterHandler(
                    async interactionContext =>
                    {
                        NewSequenzerDialogViewModel.DialogResult result = await Task<NewSequenzerDialogViewModel.DialogResult>.Run(() =>
                        {
                            NewSequenzerDialogView dialogView = new NewSequenzerDialogView();
                            dialogView.DataContext = new NewSequenzerDialogViewModel();
                            if (dialogView.ShowDialog() == true)
                                return (dialogView.DataContext as NewSequenzerDialogViewModel).GetResult();

                            return null;
                        });

                        interactionContext.SetOutput(result); //Der erste der SetOutput benutzt, wird verwendet
                    });*/

                this
                    .ViewModel
                    .NewSequenzerDialog
                    .RegisterHandler(
                    interactionContext =>
                    {
                        NewSequenzerDialogView dialogView = new NewSequenzerDialogView();
                        dialogView.DataContext = new NewSequenzerDialogViewModel();
                        NewSequenzerDialogViewModel.DialogResult result = null;
                        if (dialogView.ShowDialog() == true)
                            result =(dialogView.DataContext as NewSequenzerDialogViewModel).GetResult();

                        interactionContext.SetOutput(result); //Der erste der SetOutput benutzt, wird verwendet
                    });
            });
        }
    }
}
