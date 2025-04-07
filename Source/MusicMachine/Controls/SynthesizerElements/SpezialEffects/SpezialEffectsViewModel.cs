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

        public bool UseDelayEffect
        {
            get { return this.model.UseDelayEffect; }
            set { this.model.UseDelayEffect = value; this.RaisePropertyChanged(nameof(UseDelayEffect)); }
        }

        public bool UseHallEffect
        {
            get { return this.model.UseHallEffect; }
            set { this.model.UseHallEffect = value; this.RaisePropertyChanged(nameof(UseHallEffect)); }
        }

        public bool UseGainEffect
        {
            get { return this.model.UseGainEffect; }
            set { this.model.UseGainEffect = value; this.RaisePropertyChanged(nameof(UseGainEffect)); }
        }

        public float Gain
        {
            get { return this.model.Gain; }
            set { this.model.Gain = value; this.RaisePropertyChanged(nameof(Gain)); }
        }

        public bool UsePitchEffect
        {
            get { return this.model.UsePitchEffect; }
            set { this.model.UsePitchEffect = value; this.RaisePropertyChanged(nameof(UsePitchEffect)); }
        }

        public float Pitch
        {
            get { return this.model.PitchEffect; }
            set { this.model.PitchEffect = value; this.RaisePropertyChanged(nameof(Pitch)); }
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
            this.UseDelayEffect = data.UseDelayEffect;
            this.UseHallEffect = data.UseHallEffect;
            this.UseGainEffect = data.UseGainEffect;
            this.Gain = data.Gain;
            this.UseVolumeLfo = data.UseVolumeLfo;
            this.VolumeLfoFrequency = data.VolumeLfoFrequency;
        }
    }
}
