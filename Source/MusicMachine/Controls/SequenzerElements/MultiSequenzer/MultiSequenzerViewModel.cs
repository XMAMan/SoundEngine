using DynamicData;
using DynamicData.Binding;
using MidiParser;
using MusicMachine.Controls.NewSequenzerDialog;
using MusicMachine.Controls.SequenzerElements.Sequenzer;
using MusicMachine.Helper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using WaveMaker;
using WaveMaker.Helper;
using WaveMaker.Sequenzer;

namespace MusicMachine.Controls.SequenzerElements.MultiSequenzer
{
    public class MultiSequenzerViewModel : ReactiveObject
    {
        private WaveMaker.Sequenzer.MultiSequenzer model;
        private IDisposable timer;
        private MidiPlayer.MidiPlayer midiPlayer = null;
        private IAudioFileHandler audioFilehandler;

        public MultiSequenzerViewModel(WaveMaker.Sequenzer.MultiSequenzer model, IAudioFileHandler audioFilehandler)
        {
            this.model = model;
            this.audioFilehandler = audioFilehandler;

            this.timer = Observable.Interval(TimeSpan.FromMilliseconds(250))
                .Subscribe(x => 
                { 
                    //Timer-Action
                    if (this.IsPlaying) this.CurrentPosition = this.model.CurrentPosition; 
                });

            //Wenn jemand mit der Maus den blauen Balken verschiebt, soll die neue Position aufs Model übertragen werden
            this.Sequenzers.ToObservableChangeSet()    
                .MergeMany(x => x.MouseMoveSamplePosition) //Imm wenn ein neuer Sequenzer der Sequenzers hinzugefügt wird, dann sollen dessen MouseMove-Events hier mit in die vereinte Event-Liste mit rein
                .Subscribe(samplePosition =>
                {
                    this.CurrentPosition = this.model.CurrentPosition = samplePosition;
                });
            
            //Lösche den Sequenzer
            this.Sequenzers.ToObservableChangeSet()
                .MergeMany(x => x.DeleteSequenzerCommand)
                .Subscribe(sequenzer =>
                {
                    this.model.RemoveSequenzer(sequenzer.Model);
                    this.Sequenzers.Remove(sequenzer); //Entferne aus ViewModel
                });

            //Markiere den Sequenzer aus der ListView, wenn dessen Canvas angeklickt wurde
            this.Sequenzers.ToObservableChangeSet()
                .MergeMany(x => x.MouseClickCanvas)
                .Subscribe(sequenzer =>
                {
                    this.SelectedSequenzer = sequenzer;
                });

            //Markiere den Sequenzer aus der ListView, wenn eine neue Note erzeugt wurde
            this.Sequenzers.ToObservableChangeSet()
                .MergeMany(x => x.CreateNoteCommand)
                .Subscribe(sequenzer =>
                {
                    this.SelectedSequenzer = sequenzer;
                });
            

            //Markiere den Sequenzer aus der ListView, wenn IsEnabled (Soll es Töne erklingen lassen?) auf true geht
            this.Sequenzers.ToObservableChangeSet()
                .MergeMany(sequenzer => sequenzer
                    .WhenAnyValue(prop => prop.IsEnabled)
                    .Where(isEnabled => isEnabled == true)
                    .Select(y => sequenzer))
                .Subscribe(sequenzer =>
                {
                    this.SelectedSequenzer = sequenzer;
                });

            //Konvertierer zwischen LengthInMillisecondsForNewCreatedNotes und MinSampleLengthForNewNotes
            this.WhenAnyValue(x => x.LengthInMillisecondsForNewCreatedNotes)
                .Select(x => (int)(x / 1000 * this.model.SampleRate))
                .ToPropertyEx(this, x => x.MinSampleLengthForNewNotes);

            this.CopyToClipboardCommand = ReactiveCommand.Create(() =>
            {
                if (this.SelectedSequenzer != null)
                {
                    this.SelectedSequenzer.CopySelectedNotesToClipboard();
                }
            });

            //Zoom-Funktion
            this.WhenAnyValue(x => x.Zoom)
                .Subscribe(x => { SetZoom(); });

            this.ExtractToClipboardCommand = ReactiveCommand.Create(() =>
            {
                if (this.SelectedSequenzer != null)
                {
                    this.SelectedSequenzer.ExtractSelectedNotesToClipboard();
                }
            });

            this.PasteFromClipboardCommand = ReactiveCommand.Create(() =>
            {
                if (this.SelectedSequenzer != null)
                {
                    this.SelectedSequenzer.PasteNotesFromeClipboard();
                }
            });

            this.MarkAllNotesCommand = ReactiveCommand.Create(() =>
            {
                if (this.SelectedSequenzer != null)
                {
                    this.SelectedSequenzer.MarkAllNotes();
                }
            });


            //Test-Tone
            this.PlayTestToneMouseDown = ReactiveCommand.Create(() =>
            {
                if (this.SelectedSequenzer != null)
                {
                    this.SelectedSequenzer.PlayTestToneMouseDown();
                }
            });
            this.PlayTestToneMouseUp = ReactiveCommand.Create(() =>
            {
                if (this.SelectedSequenzer != null)
                {
                    this.SelectedSequenzer.PlayTestToneMouseUp();
                }
            });


            this.AddEmptySequenzerCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var result = await this.NewSequenzerDialog.Handle(Unit.Default);
                if (result != null)
                {
                    AddEmptySequenzer(new SequenzerSize(result.MinOctave * 12, result.MaxOctave * 12, result.LengthInSeconds * this.model.SampleRate)); //Der leerer Sequenzer ist 25 Sekunden lang
                }
            });

            this.OpenMidiFileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                string fileName = await this.OpenFileDialog.Handle("Midi files (*.mid)|*.mid|All files (*.*)|*.*");
                if (fileName != null)
                {
                    AddMidiFile(MidiParser.MidiFile.FromFile(fileName));
                }
            });

            //Save-Button-Handler
            this.SaveCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                string fileName = await this.SaveFileDialog.Handle("Music Machine (*.music)|*.music|All files (*.*)|*.*");
                if (fileName != null)
                {
                    var data = this.model.GetAllSettings();
                    data.LengthInMillisecondsForNewCreatedNotes = this.LengthInMillisecondsForNewCreatedNotes;
                    data.SnapToGrid = this.SnapToGrid;
                    //data.MidiFileFileName = FileNameHelper.GetPathRelativeToCurrentDirectory(fileName);
                    data.MidiFileFileName = this.model.ContainsAnyNotes() ? Path.GetFileNameWithoutExtension(fileName) + ".mid" : "";
                    XmlHelper.WriteToXmlFile(data, fileName);

                    //Speichere die mid-Datei mit den Noten nur, wenn es auch Daten enthält
                    if (data.MidiFileFileName != "")
                    {
                        var midiFile = this.model.GetNotesAsMidiFile();
                        midiFile.WriteToFile(Path.GetDirectoryName(fileName) + "\\" + data.MidiFileFileName);
                    }                    

                    this.CurrentProjectName = Path.GetFileNameWithoutExtension(fileName);
                }
            });

            //Load-Button-Handler
            this.LoadCommand = ReactiveCommand.CreateFromTask(async () => 
            {
                string fileName = await this.OpenFileDialog.Handle("Music Machine (*.music)|*.music|All files (*.*)|*.*");
                if (fileName != null)
                {
                    var data = XmlHelper.LoadFromXmlFile<MultiSequenzerData>(fileName);
                    this.model.SetAllSettings(data, Path.GetDirectoryName(fileName), this.audioFilehandler);
                    
                    this.LengthInMillisecondsForNewCreatedNotes = data.LengthInMillisecondsForNewCreatedNotes;
                    this.SnapToGrid = data.SnapToGrid;

                    this.Sequenzers.Clear();

                    foreach (var sequenzerModel in this.model.GetAllSequenzers())
                    {
                        this.Sequenzers.Add(new SequenzerViewModel(sequenzerModel, this.audioFilehandler));
                    }
                    this.SelectedSequenzer = this.Sequenzers[0];
                    this.SelectedSequenzer.SynthesizerViewModel.SetAllSettings(data.SynthesizerData[0].SynthesizerData, Path.GetDirectoryName(fileName));
                    SetZoom();

                    this.CurrentPosition = this.model.CurrentPosition = 0;

                    this.CurrentProjectName = Path.GetFileNameWithoutExtension(fileName);
                }
            });

            //PlayMidi
            this.PlayMidiCommand = ReactiveCommand.Create(() =>
            {
                if (this.midiPlayer != null)
                {
                    this.midiPlayer.Stop();
                }

                var midiFile = this.model.GetNotesAsMidiFile();

                this.midiPlayer = new MidiPlayer.MidiPlayer(midiFile);
                this.midiPlayer.Start();
            });

            //Stop Midi
            this.StopMidiCommand = ReactiveCommand.Create(() =>
            {
                if (this.midiPlayer != null)
                {
                    this.midiPlayer.Stop();
                    this.midiPlayer = null;
                }
            });

            //Als wav/mp3 speichern
            this.ExportAudioDataCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                string fileName = await this.SaveFileDialog.Handle("mp3 (*.mp3)|*.mp3|Wave Format(*.wav)|*.wav|All files (*.*)|*.*");
                if (fileName != null)
                {
                    this.audioFilehandler.ExportAudioStreamToFile(this.model.GetAllSamples(), this.model.SampleRate, fileName);
                }
            });

            this.MouseDoubleClickOnKeyStrokeSpeedCommand = ReactiveCommand.Create(() =>
            {
                this.KeyStrokeSpeed = 1;
            });
        }

        public ObservableCollection<SequenzerViewModel> Sequenzers { get; set; } = new ObservableCollection<SequenzerViewModel>();
        [Reactive] public SequenzerViewModel SelectedSequenzer { get; set; } = null;

        public bool IsPlaying
        {
            get { return this.model.IsRunning; }
            set { this.model.IsRunning = value; }
        }

        public bool AutoLoop
        {
            get { return this.model.AutoLoop; }
            set { this.model.AutoLoop = value; }
        }

        public float Volume
        {
            get { return this.model.Volume; }
            set { this.model.Volume = value; }
        }

        public float KeyStrokeSpeed
        {
            get { return this.model.KeyStrokeSpeed; }
            set { this.model.KeyStrokeSpeed = value; }
        }
        

        [Reactive] public float LengthInMillisecondsForNewCreatedNotes { get; set; } = 250; //So lang sind neu erstellte Noten
        [ObservableAsProperty] public int MinSampleLengthForNewNotes { get; private set; }
        [Reactive] public bool SnapToGrid { get; set; } = true; //Sollen neu eingefügte Noten am Gitter ausgerichtet werden?
        [Reactive] public bool Zoom { get; set; } = false;
        [Reactive] public SequenzerSize SequenzerSize { get; private set; }
        [Reactive] public string CurrentProjectName { get; set; } = "Music Machine";

        [Reactive] public double CurrentPosition { get; set; } = 0; //Geht von 0 bis SampleCount

        public ReactiveCommand<Unit, Unit> MouseDoubleClickOnKeyStrokeSpeedCommand { get; private set; }

        [Reactive] public int FrequenceForTestTone { get; set; } = 65; 

        private double sequencerPixelWidth = double.NaN; //NaN entspricht Auto
        public double SequencerPixelWidth
        {
            get => sequencerPixelWidth;
            set => this.RaiseAndSetIfChanged(ref sequencerPixelWidth, value == 0 ? double.NaN : value);
        }
        [Reactive] public double SequencerPixelHeight { get; set; } = 240;

        public void AddMidiFile(MidiFile file)
        {
            var sequenzerModels = model.AddMidiFile(file);
            AddSequenzers(sequenzerModels.Select(x => new SequenzerViewModel(x, this.audioFilehandler)));
        }
        public void AddEmptySequenzer(SequenzerSize size)
        {
            var sequenzerModel = this.model.AddEmptySequenzer(size);
            AddSequenzer(new SequenzerViewModel(sequenzerModel, this.audioFilehandler));
        }

        private void AddSequenzer(SequenzerViewModel newItem)
        {
            AddSequenzers(new SequenzerViewModel[] { newItem });
        }

        private void AddSequenzers(IEnumerable<SequenzerViewModel> newItems)
        {
            this.Sequenzers.AddRange(newItems);

            if (this.SelectedSequenzer == null) this.SelectedSequenzer = this.Sequenzers[0];
            SetZoom();

        }

        private void SetZoom()
        {
            this.SequenzerSize = this.Zoom ? this.model.CurrentNoteSize : this.model.MaxAllowedSize;
            if (this.Zoom) this.SequencerPixelWidth = double.NaN;
        }

        public void SetKeyEventHandler(IObservable<KeyEventArgs> keyDown, IObservable<KeyEventArgs> keyUp)
        {
            keyDown.WhereLastKeys(Key.LeftCtrl, Key.C)
                    .InvokeCommand(this.CopyToClipboardCommand);

            keyDown.WhereLastKeys(Key.LeftCtrl, Key.X)
                    .InvokeCommand(this.ExtractToClipboardCommand);

            keyDown.WhereLastKeys(Key.LeftCtrl, Key.V)
                    .InvokeCommand(this.PasteFromClipboardCommand);

            keyDown.WhereLastKeys(Key.LeftCtrl, Key.A)
                    .InvokeCommand(this.MarkAllNotesCommand);

            //keyDown.WhereLastKeys(Key.A)
            //        .InvokeCommand(this.PlayTestToneMouseDown);

            //keyUp.WhereLastKeys(Key.A)
            //       .InvokeCommand(this.PlayTestToneMouseUp);

            keyDown
                .Select(x => KeyToSequenzerIndex(x.Key))
                .Where(x => x >= 0 && x < this.Sequenzers.Count)
                .Subscribe(x => this.Sequenzers[x].PlayTestToneMouseDown());

            keyUp
                .Select(x => KeyToSequenzerIndex(x.Key))
                .Where(x => x >= 0 && x < this.Sequenzers.Count)
                .Subscribe(x => this.Sequenzers[x].PlayTestToneMouseUp());
        }

        private static int KeyToSequenzerIndex(Key key)
        {
            switch(key)
            {
                case Key.A:
                    return 0;
                case Key.S:
                    return 1;
                case Key.D:
                    return 2;
            }
            return -1;
        }

        public ReactiveCommand<Unit, Unit> CopyToClipboardCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> ExtractToClipboardCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> PasteFromClipboardCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> MarkAllNotesCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> OpenMidiFileCommand { get; private set; }        
        public ReactiveCommand<Unit, Unit> AddEmptySequenzerCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> SaveCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> LoadCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> PlayMidiCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> StopMidiCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> ExportAudioDataCommand { get; private set; }        
        public Interaction<string, string> OpenFileDialog { get; private set; } = new Interaction<string, string>(); //Input: Filter (openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";); Output: Dateiname von der Datei die geöffnet werden soll
        public Interaction<string, string> SaveFileDialog { get; private set; } = new Interaction<string, string>(); //Input: Filter (openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";); Output: Dateiname von der Datei, die erzeugt werden soll
        public ReactiveCommand<Unit, Unit> PlayTestToneMouseDown { get; private set; }
        public ReactiveCommand<Unit, Unit> PlayTestToneMouseUp { get; private set; }
        public Interaction<Unit, NewSequenzerDialogViewModel.DialogResult> NewSequenzerDialog { get; private set; } = new Interaction<Unit, NewSequenzerDialogViewModel.DialogResult>();
    }
}
