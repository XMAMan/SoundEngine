using ReactiveUI;
using WaveMaker.KeyboardComponents;

namespace MusicMachine.Controls.SynthesizerElements.Filter
{
    public class FilterViewModel : ReactiveObject
    {
        private Synthesizer model;
        public FilterViewModel(Synthesizer model)
        {
            this.model = model;
        }

        public bool IsLowPassEnabled
        {
            get { return this.model.IsLowPassEnabled; }
            set { this.model.IsLowPassEnabled = value; this.RaisePropertyChanged(nameof(IsLowPassEnabled)); }
        }
        public float LowPassCutOffFrequence
        {
            get { return this.model.LowPassCutOffFrequence; }
            set { this.model.LowPassCutOffFrequence = value; this.RaisePropertyChanged(nameof(LowPassCutOffFrequence)); }
        }
        public float LowPassResonance
        {
            get { return this.model.LowPassResonance; }
            set { this.model.LowPassResonance = value; this.RaisePropertyChanged(nameof(LowPassResonance)); }
        }

        public bool IsHighPassEnabled
        {
            get { return this.model.IsHighPassEnabled; }
            set { this.model.IsHighPassEnabled = value; this.RaisePropertyChanged(nameof(IsHighPassEnabled)); }
        }
        public float HighPassCutOffFrequence
        {
            get { return 1 - this.model.HighPassCutOffFrequence; }
            set { this.model.HighPassCutOffFrequence = 1 - value; this.RaisePropertyChanged(nameof(HighPassCutOffFrequence)); }
        }
        public float HighPassResonance
        {
            get { return this.model.HighPassResonance; }
            set { this.model.HighPassResonance = value; this.RaisePropertyChanged(nameof(HighPassResonance)); }
        }


        public void SetAllSettings(SynthesizerData data)
        {
            this.IsLowPassEnabled = data.IsLowPassEnabled;
            this.LowPassCutOffFrequence = data.LowPassCutOffFrequence;
            this.LowPassResonance = data.LowPassResonance;
            this.IsHighPassEnabled = data.IsHighPassEnabled;
            this.HighPassCutOffFrequence = data.HighPassCutOffFrequence;
            this.HighPassResonance = data.HighPassResonance;
        }

    }
}
