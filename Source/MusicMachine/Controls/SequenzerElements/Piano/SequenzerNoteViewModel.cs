using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Media;
using WaveMaker.Sequenzer;
using static MusicMachine.Controls.SequenzerElements.Sequenzer.RectangleNotes;

namespace MusicMachine.Controls.SequenzerElements.Piano
{
    //ViewModel für eine einzelne Note/Rechteck aus dem Sequenzer
    public class SequenzerNoteViewModel : ReactiveObject
    {
        public SequenzerKey Model { get; private set; }

        [ObservableAsProperty] public Brush Color { get; private set; }

        [Reactive] public bool IsMarked { get; set; } = false; //Wurde es mit der Maus markiert?

        [Reactive] public double LeftPosition { get; set; }
        [Reactive] public double TopPosition { get; set; }
        [Reactive] public double Width { get; set; }

        public SequenzerNoteViewModel(SequenzerKey model)
        {
            this.Model = model;

            //Note wurde angeklickt
            this.NoteClickCommand = ReactiveCommand.Create<Unit,SequenzerNoteViewModel>((_) =>
            {
                this.IsMarked = !this.IsMarked;
                return this;
            });

            //Note wurde verschoben / Linker oder Rechter Rand wurde verschoben
            this.NoteMoveCommand = ReactiveCommand.Create<MouseNoteEventArgs, SequenzerNoteViewModel>((args) =>
            {
                //Aktualisiere Model
                this.Model.StartByteIndex = args.SamplePosition;
                this.Model.NoteNumber = args.ToneIndex;
                this.Model.Length = args.SampleLength;

                return this;
            });

            //MouseUp-Event nachdem Note verschoben wurde (Spiele Ton)
            this.NoteMouseUpCommand = ReactiveCommand.Create<Unit, SequenzerNoteViewModel>((_) =>
            {
                //this.IsMarked = !this.IsMarked;
                return this;
            });

            //Delete-Command
            this.NoteDeleteCommand = ReactiveCommand.Create<Unit, SequenzerNoteViewModel>((_) =>
            {
                return this;
            });

            //Ändere Farbe, wenn Markerungszustand sich ändert
            this.WhenAnyValue(x => x.IsMarked)
                .Select(x => x ? Brushes.Turquoise : Brushes.Red)
                .ToPropertyEx(this, x => x.Color);
        }

        public ReactiveCommand<Unit, SequenzerNoteViewModel> NoteClickCommand { get; private set; }
        public ReactiveCommand<MouseNoteEventArgs, SequenzerNoteViewModel> NoteMoveCommand { get; private set; }
        public ReactiveCommand<Unit, SequenzerNoteViewModel> NoteMouseUpCommand { get; private set; }
        public ReactiveCommand<Unit, SequenzerNoteViewModel> NoteDeleteCommand { get; private set; }
    }
}
