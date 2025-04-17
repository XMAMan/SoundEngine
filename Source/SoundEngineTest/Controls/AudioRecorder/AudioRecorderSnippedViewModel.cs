using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using SoundEngine.SoundSnippeds;
using System.Reactive;
using WaveMaker;
using System.Reactive.Linq;
using SoundEngine;

namespace SoundEngineTest.Controls.AudioRecorder
{
    public class AudioRecorderSnippedViewModel : ReactiveObject
    {
        private IAudioRecorderSnipped snipp;
        private IAudioFileWriter audioFileWriter;
        private int sampleRate;

        private List<float> recordData = new List<float>();

        public AudioRecorderSnippedViewModel(ISoundGenerator soundGenerator)
        {
            this.snipp = soundGenerator.AudioRecorder;
            this.audioFileWriter = soundGenerator.AudioFileWriter;
            this.sampleRate = soundGenerator.SampleRate;
            soundGenerator.AudioOutputCallback += SoundGenerator_AudioOutputCallback;

            this.snipp.IsRunningChanged = (isRunning) => { IsRunning = isRunning; };

            StartCommand = ReactiveCommand.Create(() =>
            {
                this.snipp.StartRecording();
            });
            StopCommand = ReactiveCommand.Create(() =>
            {
                this.snipp.StopRecording();
            });

            if (SignalSources.Any())
            {
                SelectedSignalSource = SignalSources.First();
            }

            StartRecording = ReactiveCommand.Create(() =>
            {
                this.recordData.Clear();
                this.IsRecording = true;
            });

            this.StopRecording = ReactiveCommand.CreateFromTask(async () =>
            {
                this.IsRecording = false;

                string fileName = await this.SaveFileDialog.Handle("mp3 (*.mp3)|*.mp3|Wave Format(*.wav)|*.wav|All files (*.*)|*.*");
                if (fileName != null)
                {
                    this.audioFileWriter.ExportAudioStreamToFile(this.recordData.ToArray(), this.sampleRate, fileName);
                }

                this.recordData.Clear();
            });
        }

        private void SoundGenerator_AudioOutputCallback(object? sender, float[] buffer)
        {
            if (this.IsRecording)
            {
                this.recordData.AddRange(buffer);
            }

            this.OutputVolume = buffer.Sum(x => Math.Abs(x)) / buffer.Length;
        }

        public IEnumerable<string> SignalSources
        {
            get
            {
                return snipp.GetAvailableDevices();
            }
        }

        public string SelectedSignalSource
        {
            get { return snipp.SelectedDevice; }
            set
            {
                snipp.SelectedDevice = value;
                this.RaisePropertyChanged(nameof(SelectedSignalSource));
            }
        }
        
        [Reactive] public bool IsRunning { get; private set; } = false;

        [Reactive] public bool IsRecording { get; private set; } = false;
        [Reactive] public double OutputVolume { get; set; } = 0;
        public ReactiveCommand<Unit, Unit> StartCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; private set; }
        public float Volume { get { return snipp.Volume; } set { snipp.Volume = value; } }

        public bool UseDelayEffect { get { return snipp.UseDelayEffect; } set { snipp.UseDelayEffect = value; } }
        public bool UseHallEffect { get { return snipp.UseHallEffect; } set { snipp.UseHallEffect = value; } }
        public bool UseGainEffect { get { return snipp.UseGainEffect; } set { snipp.UseGainEffect = value; } }
        public float Gain { get { return snipp.Gain; } set { snipp.Gain = value; } }
        public bool UsePitchEffect { get { return snipp.UsePitchEffect; } set { snipp.UsePitchEffect = value; } }
        public float PitchEffect { get { return snipp.PitchEffect; } set { snipp.PitchEffect = value; } }
        public bool UseVolumeLfo { get { return snipp.UseVolumeLfo; } set { snipp.UseVolumeLfo = value; } }
        public float VolumeLfoFrequency { get { return snipp.VolumeLfoFrequency; } set { snipp.VolumeLfoFrequency = value; } }

        public ReactiveCommand<Unit, Unit> StartRecording { get; private set; }
        public ReactiveCommand<Unit, Unit> StopRecording { get; private set; }
        public Interaction<string, string> SaveFileDialog { get; private set; } = new Interaction<string, string>(); //Input: Filter (openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";); Output: Dateiname von der Datei, die erzeugt werden soll
    }
}
