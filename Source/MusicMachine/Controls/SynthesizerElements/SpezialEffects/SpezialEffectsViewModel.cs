using ReactiveUI;
using WaveMaker.KeyboardComponents;

namespace MusicMachine.Controls.SynthesizerElements.SpezialEffects
{
    public class SpezialEffectsViewModel : ReactiveObject
    {
        private Synthesizer model;
        public SpezialEffectsViewModel(Synthesizer model)
        {
            this.model = model;
            this.Gain = 7;
            this.VolumeLfoFrequency = 20;
        }

        public bool UseDelayEffekt
        {
            get { return this.model.UseDelayEffekt; }
            set { this.model.UseDelayEffekt = value; this.RaisePropertyChanged(nameof(UseDelayEffekt)); }
        }

        public bool UseHallEffekt
        {
            get { return this.model.UseHallEffekt; }
            set { this.model.UseHallEffekt = value; this.RaisePropertyChanged(nameof(UseHallEffekt)); }
        }

        public bool UseGainEffekt
        {
            get { return this.model.UseGainEffekt; }
            set { this.model.UseGainEffekt = value; this.RaisePropertyChanged(nameof(UseGainEffekt)); }
        }

        public float Gain
        {
            get { return this.model.Gain; }
            set { this.model.Gain = value; this.RaisePropertyChanged(nameof(Gain)); }
        }

        public bool UseVolumeLfo
        {
            get { return this.model.UseVolumeLfo; }
            set { this.model.UseVolumeLfo = value; this.RaisePropertyChanged(nameof(UseVolumeLfo)); }
        }

        public float VolumeLfoFrequency
        {
            get { return this.model.VolumeLfoFrequency; }
            set { this.model.VolumeLfoFrequency = value; this.RaisePropertyChanged(nameof(VolumeLfoFrequency)); }
        }

        public void SetAllSettings(SynthesizerData data)
        {
            this.UseDelayEffekt = data.UseDelayEffekt;
            this.UseHallEffekt = data.UseHallEffekt;
            this.UseGainEffekt = data.UseGainEffekt;
            this.Gain = data.Gain;
            this.UseVolumeLfo = data.UseVolumeLfo;
            this.VolumeLfoFrequency = data.VolumeLfoFrequency;
        }
    }
}
