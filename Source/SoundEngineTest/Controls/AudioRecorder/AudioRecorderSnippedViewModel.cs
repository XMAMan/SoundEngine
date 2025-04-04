using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using SoundEngine.SoundSnippeds;
using System.Reactive;

namespace SoundEngineTest.Controls.AudioRecorder
{
    public class AudioRecorderSnippedViewModel : ReactiveObject
    {
        private IAudioRecorderSnipped snipp;
        public AudioRecorderSnippedViewModel(IAudioRecorderSnipped snipp)
        {
            this.snipp = snipp;

            this.snipp.IsRunningChanged = (isRunning) => { IsRunning = isRunning; };

            Play = ReactiveCommand.Create(() =>
            {
                this.snipp.Play();
            });
            Stop = ReactiveCommand.Create(() =>
            {
                this.snipp.Stop();
            });

            if (SignalSources.Any())
            {
                SelectedSignalSource = SignalSources.First();
            }
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
        public ReactiveCommand<Unit, Unit> Play { get; private set; }
        public ReactiveCommand<Unit, Unit> Stop { get; private set; }
        public float Volume { get { return snipp.Volume; } set { snipp.Volume = value; } }

        public bool UseDelayEffekt { get { return snipp.UseDelayEffekt; } set { snipp.UseDelayEffekt = value; } }
        public bool UseHallEffekt { get { return snipp.UseHallEffekt; } set { snipp.UseHallEffekt = value; } }
        public bool UseGainEffekt { get { return snipp.UseGainEffekt; } set { snipp.UseGainEffekt = value; } }
        public float Gain { get { return snipp.Gain; } set { snipp.Gain = value; } }
        public bool UseVolumeLfo { get { return snipp.UseVolumeLfo; } set { snipp.UseVolumeLfo = value; } }
        public float VolumeLfoFrequency { get { return snipp.VolumeLfoFrequency; } set { snipp.VolumeLfoFrequency = value; } }
    }
}
