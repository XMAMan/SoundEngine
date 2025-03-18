using ReactiveUI;
using System.Collections.Generic;
using System;
using WaveMaker;
using WaveMaker.KeyboardComponents;
using System.Linq;
using ReactiveUI.Fody.Helpers;
using System.Windows;

namespace MusicMachine.ViewModel.SynthesizerVM
{
    public class SynthesizerViewModel : ReactiveObject
    {
        private Synthesizer model;
        public SynthesizerViewModel(Synthesizer model, IAudioFileReader audioFileReader)
        {
            this.model = model;
            this.OsziViewModel = new OscillatorViewModel(model);
            this.AudioFileViewModel = new AudioFileViewModel(model, audioFileReader);
            this.SpezialEffectsViewModel = new SpezialEffectsViewModel(model);
            this.FilterViewModel = new FilterViewModel(model);
            this.AdsrEnvelope = new AdsrEnvelopeViewModel(model);
        }

        public enum SignalSource
        {
            Oscillator,
            AudioFile
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
            get { return this.model.UseDataFromAudioFileInsteadFromOszi ? SignalSource.AudioFile : SignalSource.Oscillator; }
            set 
            { 
                this.model.UseDataFromAudioFileInsteadFromOszi = (value == SignalSource.AudioFile ? true : false);                
                this.RaisePropertyChanged(nameof(SelectedSignalSource));
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            this.OscillatorVisibility = this.model.UseDataFromAudioFileInsteadFromOszi ? Visibility.Collapsed : Visibility.Visible;
            this.AudioFileVisibility = this.model.UseDataFromAudioFileInsteadFromOszi ? Visibility.Visible : Visibility.Collapsed;
        }

        [Reactive] public Visibility OscillatorVisibility { get; private set; } = Visibility.Visible;
        [Reactive] public Visibility AudioFileVisibility { get; private set; } = Visibility.Collapsed;
        

        public OscillatorViewModel OsziViewModel { get; private set; }
        public AudioFileViewModel AudioFileViewModel { get; private set; }
        public FilterViewModel FilterViewModel { get; private set; }
        public AdsrEnvelopeViewModel AdsrEnvelope { get; private set; }
        public SpezialEffectsViewModel SpezialEffectsViewModel { get; private set; }

        public SynthesizerData GetAllSettings()
        {
            return this.model.GetAllSettings();
        }
        public void SetAllSettings(SynthesizerData data, string searchDirectoryForAudioFiles)
        {
            this.OsziViewModel.SetAllSettings(data);
            this.AudioFileViewModel.SetAllSettings(data, searchDirectoryForAudioFiles);
            this.FilterViewModel.SetAllSettings(data);
            this.AdsrEnvelope.SetAllSettings(data);
            this.SpezialEffectsViewModel.SetAllSettings(data);
        }
    }
}
