using ReactiveUI;
using WaveMaker.KeyboardComponents;

namespace MusicMachine.ViewModel.SynthesizerVM
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
        public float CutOffFrequence
        {
            get { return this.model.CutOffFrequence; }
            set { this.model.CutOffFrequence = value; this.RaisePropertyChanged(nameof(CutOffFrequence)); }
        }
        public float Resonance
        {
            get { return this.model.Resonance; }
            set { this.model.Resonance = value; this.RaisePropertyChanged(nameof(Resonance)); }
        }



        public void SetAllSettings(SynthesizerData data)
        {
            this.IsLowPassEnabled = data.IsLowPassEnabled;
            this.CutOffFrequence = data.CutOffFrequence;
            this.Resonance = data.Resonance;
        }
    }
}
