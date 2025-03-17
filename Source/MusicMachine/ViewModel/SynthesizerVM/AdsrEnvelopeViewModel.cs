using ReactiveUI;
using WaveMaker.KeyboardComponents;

namespace MusicMachine.ViewModel.SynthesizerVM
{
    public class AdsrEnvelopeViewModel : ReactiveObject
    {
        private Synthesizer model;
        public AdsrEnvelopeViewModel(Synthesizer model)
        {
            this.model = model;
        }

        public float AttackTimeInMs
        {
            get { return this.model.AttackTimeInMs; }
            set { this.model.AttackTimeInMs = value; this.RaisePropertyChanged(nameof(AttackTimeInMs)); }
        }
        public float DecayTimeInMs
        {
            get { return this.model.DecayTimeInMs; }
            set { this.model.DecayTimeInMs = value; this.RaisePropertyChanged(nameof(DecayTimeInMs)); }
        }
        public float SustainVolume
        {
            get { return this.model.SustainVolume; }
            set { this.model.SustainVolume = value; this.RaisePropertyChanged(nameof(SustainVolume)); }
        }
        public float ReleaseTimeInMs
        {
            get { return this.model.ReleaseTimeInMs; }
            set { this.model.ReleaseTimeInMs = value; this.RaisePropertyChanged(nameof(ReleaseTimeInMs)); }
        }


        public void SetAllSettings(SynthesizerData data)
        {
            this.AttackTimeInMs = data.AttackTimeInMs;
            this.DecayTimeInMs = data.DecayTimeInMs;
            this.SustainVolume = data.SustainVolume;
            this.ReleaseTimeInMs = data.ReleaseTimeInMs;
        }
    }
}
