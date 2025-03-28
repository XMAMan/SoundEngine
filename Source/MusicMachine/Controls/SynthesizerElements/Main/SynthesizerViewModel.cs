using ReactiveUI;
using WaveMaker;
using WaveMaker.KeyboardComponents;
using ReactiveUI.Fody.Helpers;
using System.Windows;
using MusicMachine.Controls.SynthesizerElements.AdsrEnvelope;
using MusicMachine.Controls.SynthesizerElements.AudioFileControl;
using MusicMachine.Controls.SynthesizerElements.Filter;
using MusicMachine.Controls.SynthesizerElements.Oscillator;
using MusicMachine.Controls.SynthesizerElements.SpezialEffects;
using MusicMachine.Controls.SynthesizerElements.MicrophoneControl;

namespace MusicMachine.Controls.SynthesizerElements.Main
{
    public class SynthesizerViewModel : ReactiveObject
    {
        private Synthesizer model;
        public SynthesizerViewModel(Synthesizer model, IAudioFileReader audioFileReader, ITestToneProvider testToneProvider)
        {
            this.model = model;
            this.OsziViewModel = new OscillatorViewModel(model);
            this.AudioFileViewModel = new AudioFileViewModel(model, audioFileReader);
            this.MicrophoneViewModel = new MicrophoneViewModel(model.AudioRecorder.AudioRecorder, testToneProvider);
            this.SpezialEffectsViewModel = new SpezialEffectsViewModel(model);
            this.FilterViewModel = new FilterViewModel(model);
            this.AdsrEnvelope = new AdsrEnvelopeViewModel(model);
        }

        public IEnumerable<SignalSource> SignalSources
        {
            get
            {
                return Enum.GetValues(typeof(SignalSource))
                    .Cast<SignalSource>();
            }
        }

        public SignalSource SelectedSignalSource
        {
            get { return this.model.SignalSource; }
            set 
            {
                this.model.SignalSource = value;
                this.RaisePropertyChanged(nameof(SelectedSignalSource));
                UpdateVisibility();
                this.MicrophoneViewModel.StopRecording();
            }
        }

        private void UpdateVisibility()
        {
            switch (this.model.SignalSource)
            {
                case SignalSource.Oscillator:
                    this.OscillatorVisibility = Visibility.Visible;
                    this.AudioFileVisibility = Visibility.Collapsed;
                    this.MicrophoneVisibility = Visibility.Collapsed;
                    break;

                case SignalSource.AudioFile:
                    this.OscillatorVisibility = Visibility.Collapsed;
                    this.AudioFileVisibility = Visibility.Visible;
                    this.MicrophoneVisibility = Visibility.Collapsed;
                    break;

                case SignalSource.Microphone:
                    this.OscillatorVisibility = Visibility.Collapsed;
                    this.AudioFileVisibility = Visibility.Collapsed;
                    this.MicrophoneVisibility = Visibility.Visible;
                    break;
            }
        }

        [Reactive] public Visibility OscillatorVisibility { get; private set; } = Visibility.Visible;
        [Reactive] public Visibility AudioFileVisibility { get; private set; } = Visibility.Collapsed;
        [Reactive] public Visibility MicrophoneVisibility { get; private set; } = Visibility.Collapsed;

        public OscillatorViewModel OsziViewModel { get; private set; }
        public AudioFileViewModel AudioFileViewModel { get; private set; }
        public MicrophoneViewModel MicrophoneViewModel { get; private set; }
        public FilterViewModel FilterViewModel { get; private set; }
        public AdsrEnvelopeViewModel AdsrEnvelope { get; private set; }
        public SpezialEffectsViewModel SpezialEffectsViewModel { get; private set; }

        public SynthesizerData GetAllSettings()
        {
            return this.model.GetAllSettings();
        }
        public void SetAllSettings(SynthesizerData data, string searchDirectoryForAudioFiles)
        {
            this.SelectedSignalSource = data.SignalSource;
            this.OsziViewModel.SetAllSettings(data);
            this.AudioFileViewModel.SetAllSettings(data, searchDirectoryForAudioFiles);
            this.FilterViewModel.SetAllSettings(data);
            this.AdsrEnvelope.SetAllSettings(data);
            this.SpezialEffectsViewModel.SetAllSettings(data);
        }
    }
}
