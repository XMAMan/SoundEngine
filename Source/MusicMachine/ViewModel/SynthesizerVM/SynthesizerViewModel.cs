using ReactiveUI;
using WaveMaker;
using WaveMaker.KeyboardComponents;

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
