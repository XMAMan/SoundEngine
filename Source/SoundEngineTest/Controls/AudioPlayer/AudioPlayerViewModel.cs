using AudioWpfControls.Helper;
using Microsoft.Win32;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SignalAnalyser;
using SoundEngine;
using SoundEngine.SoundSnippeds;
using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;

namespace SoundEngineTest.Controls.AudioPlayer
{
    public class AudioPlayerViewModel : ReactiveObject
    {
        private string WorkingDirectory = @"..\..\..\..\..\Data\";

        private IAudioFileSnipped audioFileSnipped = null;
        private AudioFileAnalyser analyser = null;
        private IDisposable timer;

        [Reactive] public double PlayPosition { get; set; } = 0;
        [Reactive] public int ImageWidth { get; set; } = 600;
        [Reactive] public int ImageHeight { get; set; } = 50;
        [Reactive] public bool FileIsLoaded { get; set; } = false;
        [Reactive] public BitmapImage SampleImage { get; set; }
        [Reactive] public float[] FrequencySpectrum { get; set; }
        [Reactive] public int YSteps { get; set; } = 20; //So viele Kästchen werden pro Säule gezeichnet
        public float Volume { get { return audioFileSnipped != null ? audioFileSnipped.Volume : 0; } set { if (audioFileSnipped != null) audioFileSnipped.Volume = value; } }
        public float Speed { get { return audioFileSnipped != null ? audioFileSnipped.Speed : 0; } set { if (audioFileSnipped != null) audioFileSnipped.Speed = value; } }

        public ReactiveCommand<Unit, Unit> OpenFileCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> PlayCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> BreakCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; private set; }

        public AudioPlayerViewModel(ISoundGenerator soundGenerator)
        {
            this.timer = Observable.Interval(TimeSpan.FromMilliseconds(100))
                .Subscribe(x =>
                {
                    //Timer-Action
                    if (this.audioFileSnipped != null)
                    {
                        double position = this.audioFileSnipped.SampleIndex / this.audioFileSnipped.SampleCount;
                        this.PlayPosition = this.ImageWidth * position;

                        if (this.analyser != null)
                        {
                            this.FrequencySpectrum = this.analyser.GetFrequenceSpectrumFromTime(position);
                        }                       
                    }
                });

            OpenFileCommand = ReactiveCommand.Create(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "mp3/wav/wma|*.mp3;*.wav;*.wma|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = System.IO.Path.GetFullPath(Environment.CurrentDirectory + "\\" + WorkingDirectory) ;
                if (openFileDialog.ShowDialog() == true)
                {
                    if (this.audioFileSnipped != null)
                    {
                        this.audioFileSnipped.Dispose();
                    }
                    this.audioFileSnipped = soundGenerator.AddSoundFile(openFileDialog.FileName);
                    this.FileIsLoaded = true;

                    this.analyser = new AudioFileAnalyser(this.audioFileSnipped.AudioFileSamples, this.audioFileSnipped.SampleRate);

                    Bitmap bitmap = SignalImageCreator.GetLowPassSignalImage(analyser, this.ImageWidth, this.ImageHeight);
                    this.SampleImage = bitmap.ToBitmapImage();
                    
                    //Da das nur Getter sind muss ich die PropertyChanged-Events manuell auslösen
                    this.RaisePropertyChanged(nameof(Volume));
                    this.RaisePropertyChanged(nameof(Speed));
                }
            });

            PlayCommand = ReactiveCommand.Create(() =>
            {
                Play();
            }, this.WhenAnyValue(x => x.FileIsLoaded));

            BreakCommand = ReactiveCommand.Create(() =>
            {
                Break();
            }, this.WhenAnyValue(x => x.FileIsLoaded));

            StopCommand = ReactiveCommand.Create(() =>
            {
                Stop();
            }, this.WhenAnyValue(x => x.FileIsLoaded));
        }

        private void Play()
        {
            if (this.audioFileSnipped == null) return;

            this.audioFileSnipped.Play();
        }

        private void Break()
        {
            if (this.audioFileSnipped == null) return;

            this.audioFileSnipped.Stop();
        }

        private void Stop()
        {
            if (this.audioFileSnipped == null) return;

            this.audioFileSnipped.Stop();
            this.audioFileSnipped.SampleIndex = 0;
        }
    }
}
