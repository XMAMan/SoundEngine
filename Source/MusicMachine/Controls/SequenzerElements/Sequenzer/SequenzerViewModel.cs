using DynamicData;
using DynamicData.Binding;
using MidiParser;
using MusicMachine.Controls.SequenzerElements.Piano;
using MusicMachine.Controls.SynthesizerElements.Main;
using MusicMachine.Controls.SynthesizerElements.MicrophoneControl;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using WaveMaker;
using WaveMaker.Helper;
using WaveMaker.KeyboardComponents;
using WaveMaker.Sequenzer;
using static MusicMachine.Controls.SequenzerElements.Sequenzer.RectangleNotes;

namespace MusicMachine.Controls.SequenzerElements.Sequenzer
{
    public class SequenzerViewModel : ReactiveObject
    {
        public PianoSequenzer Model { get; private set; }
        public SynthesizerViewModel SynthesizerViewModel { get; private set; }
        public PianoViewModel PianoViewModel { get; private set; }

        private int testTonekeyIndex = -1;

        public SequenzerViewModel(PianoSequenzer model, IAudioFileReader audioFileReader, ITestToneProvider testToneProvider)
        {
            this.Model = model;

            this.SynthesizerViewModel = new SynthesizerViewModel(model.Synthesizer, audioFileReader, testToneProvider);
            this.PianoViewModel = new PianoViewModel(model);

            //Spiele Ton, wenn er angeklickt wird
            this.SequenzerKeys
                .ToObservableChangeSet()
                .MergeMany(x => x.NoteClickCommand)
                .Subscribe(x =>
                {
                    this.Model.PlayTone(x.Model);
                });

            //Signal-Geber für MouseClick aufs Canvas (Wird zum markieren genutzt)
            this.MouseClickCanvas = ReactiveCommand.Create<Unit, SequenzerViewModel>((args) => { return this; });

            //Save-Button-Handler
            this.SaveSynthesizerCommand = ReactiveCommand.CreateFromTask(async () => // Create a command that calls command A.
            {
                string fileName = await this.SaveFileDialog.Handle("Synti files (*.synt)|*.synt|All files (*.*)|*.*");
                if (fileName != null)
                {
                    var data = this.SynthesizerViewModel.GetAllSettings();
                    XmlHelper.WriteToXmlFile(data, fileName);
                }
            });

            //Load-Button-Handler
            this.LoadSynthesizerCommand = ReactiveCommand.CreateFromTask(async () => // Create a command that calls command A.
            {
                string fileName = await this.OpenFileDialog.Handle("Synti files (*.synt)|*.synt|All files (*.*)|*.*");
                if (fileName != null)
                {
                    var data = XmlHelper.LoadFromXmlFile<SynthesizerData>(fileName);
                    this.SynthesizerViewModel.SetAllSettings(data, Path.GetDirectoryName(fileName)); //Spiele Änderungen vom ViewModel ins Model, indem im ViewModel alle Propertys gesetzt werden
                }
            });

            //Delete-Button-Handler
            this.DeleteSequenzerCommand = ReactiveCommand.Create<Unit, SequenzerViewModel>(args =>
            {
                return this;
            });

            //Füge neue Noten ein, wenn in freien Bereich im Canvas geklickt wurde
            this.CreateNoteCommand = ReactiveCommand.Create<MouseNoteEventArgs, SequenzerViewModel>(args => 
            {
                var key = CreateNewNote(args);
                this.Model.PlayTone(key);
                return this;
            });

            //Verschiebe Note
            this.SequenzerKeys.ToObservableChangeSet()
                .MergeMany(x => x.NoteMoveCommand)
                .Subscribe(note =>
                {
                    UpdateModelAfterNoteChanges(); //Sortiere die Noten neu nachdem sie verschoben wurden
                });

            //MouseUp-Event nachdem Note verschoben wurde
            this.SequenzerKeys.ToObservableChangeSet()
                .MergeMany(x => x.NoteMouseUpCommand)
                .Subscribe(note =>
                {
                    this.Model.PlayTone(note.Model);
                });

            //Lösche Note (mit rechter Maustaste)
            this.SequenzerKeys.ToObservableChangeSet()
                .MergeMany(x => x.NoteDeleteCommand)
                .Subscribe(note =>
                {
                    //Entferne aus ViewModel
                    this.SequenzerKeys.Remove(note);

                    //Entferne aus Model                    
                    UpdateModelAfterNoteChanges();
                });

            this.SequenzerKeys.AddRange(model.Notes.Notes.Select(x => new SequenzerNoteViewModel(x)));
        }

        public void PlayTestToneMouseDown()
        {
            if (this.testTonekeyIndex == -1)
            {
                this.testTonekeyIndex = this.StartPlayingKey(this.TestToneFrequence);
            }
        }

        public void PlayTestToneMouseUp()
        {
            if (this.testTonekeyIndex != -1)
            {
                this.ReleaseKey(this.testTonekeyIndex);
                this.testTonekeyIndex = -1;
            }
        }

        private void UpdateModelAfterNoteChanges()
        {
            this.Model.UpdateNotes(this.SequenzerKeys.OrderBy(x => x.Model.StartByteIndex).Select(x => x.Model).ToArray());
        }

        public ReactiveCommand<Unit, SequenzerViewModel> MouseClickCanvas { get; private set; }

        

        public float Volume
        {
            get { return this.Model.Volume; }
            set { this.Model.Volume = value; this.RaisePropertyChanged(nameof(Volume)); }
        }
        
        public bool IsEnabled
        {
            get { return this.Model.IsEnabled; }
            set { this.Model.IsEnabled = value; this.RaisePropertyChanged(nameof(IsEnabled)); }
        }
        public float TestToneFrequence //Wenn man Taste A drückt oder auf den Test-Ton-Button drückt
        {
            get { return this.Model.TestToneFrequence; }
            set 
            { 
                this.Model.TestToneFrequence = value;
                if (this.testTonekeyIndex != -1) SetFrequencyFromPlayingTone(this.testTonekeyIndex, value);
                this.RaisePropertyChanged(nameof(TestToneFrequence)); 
            }
        }

        [Reactive] public int MouseToneIndex { get; set; } //Über diesen ToneIndex ist die Maus gerade

        public GeneralMidiInstrument InstrumentName { get { return this.Model.InstrumentName; } }

        public ObservableCollection<SequenzerNoteViewModel> SequenzerKeys { get; private set; } = new ObservableCollection<SequenzerNoteViewModel>();


        //Input: SamplePosition, welche das SequenzerCanvas-Control schickt; Ouput: SamplePosition, welcher von diesen SequenzerViewModel als Set-Value fürs Model gemeldet wird
        public ReactiveCommand<int, int> MouseMoveSamplePosition { get; private set; } = ReactiveCommand.Create<int, int>(i => i); //Wäre es nicht besser, wenn über Interface direkt Änderung an MultiSequenzer übergeben wird? Nein, da neben Model auch MultiSequenzerViewModel aktualisiert werden muss


        public ReactiveCommand<MouseNoteEventArgs, SequenzerViewModel> CreateNoteCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveSynthesizerCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> LoadSynthesizerCommand { get; private set; }
        public ReactiveCommand<Unit, SequenzerViewModel> DeleteSequenzerCommand { get; private set; }

        public Interaction<string, string> OpenFileDialog { get; private set; } = new Interaction<string, string>(); //Input: Filter (openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";); Output: Dateiname von der Datei die geöffnet werden soll
        public Interaction<string, string> SaveFileDialog { get; private set; } = new Interaction<string, string>(); //Input: Filter (openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";); Output: Dateiname von der Datei, die erzeugt werden soll

        public void CopySelectedNotesToClipboard()
        {
            //Kopiere markierte Noten in Zwischenablage
            Clipboard.SetData("Notes", new ClipboardData() { Data = this.SequenzerKeys.Where(x => x.IsMarked).Select(x => x.Model).ToArray() });
        }

        public void ExtractSelectedNotesToClipboard()
        {
            //Schneide Noten aus und kopiere sie in Zwischenablage
            Clipboard.SetData("Notes", new ClipboardData() { Data = this.SequenzerKeys.Where(x => x.IsMarked).Select(x => x.Model).ToArray() });

            //Entferne aus ViewModel
            var toRemove = this.SequenzerKeys.Where(x => x.IsMarked).ToArray();
            foreach (var key in toRemove)
                this.SequenzerKeys.Remove(key);

            //Entferne aus Model
            UpdateModelAfterNoteChanges();
        }

        public void PasteNotesFromeClipboard()
        {
            //Füge Noten aus Zwischenablage ein
            if (Clipboard.ContainsData("Notes"))
            {
                var newKeys = ((ClipboardData)Clipboard.GetData("Notes")).Data as SequenzerKey[];

                //Entferne vorher alle Markierungen, bevor eingefügt wird
                foreach (var key in this.SequenzerKeys)
                {
                    key.IsMarked = false;
                }

                //Füge in ViewModel ein
                this.SequenzerKeys.AddRange(newKeys.Select(x => new SequenzerNoteViewModel(x) { IsMarked = true }));

                //Füge in Model ein
                UpdateModelAfterNoteChanges();
            }
        }

        private SequenzerKey CreateNewNote(MouseNoteEventArgs args)
        {
            SequenzerKey key = new SequenzerKey()
            {
                StartByteIndex = args.SamplePosition,
                Length = args.SampleLength,         
                Volume = 1,
                NoteNumber = args.ToneIndex
            };

            //Füge in ViewModel ein
            this.SequenzerKeys.Add(new SequenzerNoteViewModel(key));

            //Füge in Model ein
            UpdateModelAfterNoteChanges();
            
            return key;            
        }

        //https://stackoverflow.com/questions/9032673/clipboard-copying-objects-to-and-from
        [Serializable] //Clipboarddaten müssen zwangsweise Serializable sein
        class ClipboardData
        {
            public object Data;
        }

        public void MarkAllNotes()
        {
            foreach (var key in this.SequenzerKeys)
            {
                key.IsMarked = true;
            }
        }

        public int StartPlayingKey(float frequence)
        {
            return this.Model.StartPlayingKey(frequence);
        }

        public void ReleaseKey(int keyIndex)
        {
            this.Model.ReleaseKey(keyIndex);
        }

        public void SetToneIndexFromPlayingTone(int keyIndex, int toneIndex)
        {
            this.Model.SetToneIndexFromPlayingTone(keyIndex, toneIndex);
        }
        public void SetFrequencyFromPlayingTone(int keyIndex, float frequency)
        {
            this.Model.SetFrequencyFromPlayingTone(keyIndex, frequency);
        }
    }
}
