using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;
using System.Windows;
using WaveMaker.KeyboardComponents;

namespace MusicMachine.Controls.SynthesizerElements.Oscillator
{
    public class OscillatorViewModel : ReactiveObject
    {
        private Synthesizer model;
        public OscillatorViewModel(Synthesizer model)
        {
            this.model = model;

            this.WhenAnyValue(x => x.SelectedSignalType)
                .Select(x => x == SignalType.Rectangle ? Visibility.Collapsed : Visibility.Visible)
                .Do(x => this.PulseWidthLfoVisibility = x)
                .Subscribe();

            this.WhenAnyValue(x => x.OsciCount)
                .Select(x => x > 1 ? Visibility.Collapsed : Visibility.Visible)
                .Do(x => this.MultiOsciPitchVisibility = x)
                .Subscribe();
        }

        [Reactive] public Visibility PulseWidthLfoVisibility { get; set; } = Visibility.Collapsed;
        [Reactive] public Visibility MultiOsciPitchVisibility { get; set; } = Visibility.Collapsed;
        
        public IEnumerable<SignalType> SignalTypes
        {
            get
            {
                return Enum.GetValues(typeof(SignalType))
                    .Cast<SignalType>();
            }
        }

        public SignalType SelectedSignalType
        {
            get { return this.model.OsciType; }
            set { this.model.OsciType = value; this.RaisePropertyChanged(nameof(SelectedSignalType)); }
        }

        public float PusleWidth
        {
            get { return this.model.PusleWidth; }
            set { this.model.PusleWidth = value; this.RaisePropertyChanged(nameof(PusleWidth)); }
        }

        public int OsciCount
        {
            get { return this.model.OsciCount; }
            set { this.model.OsciCount = value; this.RaisePropertyChanged(nameof(OsciCount)); }
        }

        public float Pitch
        {
            get { return this.model.MultiOsciPitch; }
            set { this.model.MultiOsciPitch = value; this.RaisePropertyChanged(nameof(Pitch)); }
        }

        public float SubOszVolume
        {
            get { return this.model.SubOszVolume; }
            set { this.model.SubOszVolume = value; this.RaisePropertyChanged(nameof(SubOszVolume)); }
        }
        
        public bool UseFrequencyLfo
        {
            get { return this.model.UseFrequencyLfo; }
            set { this.model.UseFrequencyLfo = value; this.RaisePropertyChanged(nameof(UseFrequencyLfo)); }
        }
        public float FrequencyLfoFrequenc
        {
            get { return this.model.FrequencyLfoFrequenc; }
            set { this.model.FrequencyLfoFrequenc = value; this.RaisePropertyChanged(nameof(FrequencyLfoFrequenc)); }
        }

        public float FrequencyLfoAmplitude
        {
            get { return this.model.FrequencyLfoAmplitude; }
            set { this.model.FrequencyLfoAmplitude = value; this.RaisePropertyChanged(nameof(FrequencyLfoAmplitude)); }
        }

        public bool UsePulsewidthLfo
        {
            get { return this.model.UsePulsewidthLfo; }
            set { this.model.UsePulsewidthLfo = value; this.RaisePropertyChanged(nameof(UsePulsewidthLfo)); }
        }
        public float PulsewidthLfoFrequence
        {
            get { return this.model.PulsewidthLfoFrequence; }
            set { this.model.PulsewidthLfoFrequence = value; this.RaisePropertyChanged(nameof(PulsewidthLfoFrequence)); }
        }

        public float PulsewidthLfoAmplitude
        {
            get { return this.model.PulsewidthLfoAmplitude; }
            set { this.model.PulsewidthLfoAmplitude = value; this.RaisePropertyChanged(nameof(PulsewidthLfoAmplitude)); }
        }

        public bool UseAmplitudeLfo
        {
            get { return this.model.UseAmplitudeLfo; }
            set { this.model.UseAmplitudeLfo = value; this.RaisePropertyChanged(nameof(UseAmplitudeLfo)); }
        }
        public float AmplitudeLfoFrequenc
        {
            get { return this.model.AmplitudeLfoFrequenc; }
            set { this.model.AmplitudeLfoFrequenc = value; this.RaisePropertyChanged(nameof(AmplitudeLfoFrequenc)); }
        }
        public float AmplitudeLfoAmplitude
        {
            get { return this.model.AmplitudeLfoAmplitude; }
            set { this.model.AmplitudeLfoAmplitude = value; this.RaisePropertyChanged(nameof(AmplitudeLfoAmplitude)); }
        }        
        public float AmplitudeLfoPulseWidth
        {
            get { return this.model.AmplitudeLfoPulseWidth; }
            set { this.model.AmplitudeLfoPulseWidth = value; this.RaisePropertyChanged(nameof(AmplitudeLfoPulseWidth)); }
        }
        
        public bool UseFrequenceRamp
        {
            get { return this.model.UseFrequenceRamp; }
            set { this.model.UseFrequenceRamp = value; this.RaisePropertyChanged(nameof(UseFrequenceRamp)); }
        }
        public float FrequencyRampFactor
        {
            get { return this.model.FrequencyRampFactor; }
            set { this.model.FrequencyRampFactor = value; this.RaisePropertyChanged(nameof(FrequencyRampFactor)); }
        }
        public int RampStepFactor
        {
            get { return this.model.RampStepFactor; }
            set { this.model.RampStepFactor = value; this.RaisePropertyChanged(nameof(RampStepFactor)); }
        }
        public bool UseAccordEffect
        {
            get { return this.model.UseAccordEffect; }
            set { this.model.UseAccordEffect = value; this.RaisePropertyChanged(nameof(UseAccordEffect)); }
        }

        public void SetAllSettings(SynthesizerData data)
        {
            this.UseFrequencyLfo = data.UseFrequencyLfo;
            this.FrequencyLfoFrequenc = data.FrequencyLfoFrequenc;
            this.FrequencyLfoAmplitude = data.FrequencyLfoAmplitude;
            this.UseFrequenceRamp = data.UseFrequenceRamp;
            this.FrequencyRampFactor = data.FrequencyRampFactor;
            this.RampStepFactor = data.RampStepFactor;
            this.UsePulsewidthLfo = data.UsePulsewidthLfo;
            this.PulsewidthLfoFrequence = data.PulsewidthLfoFrequence;
            this.PulsewidthLfoAmplitude = data.PulsewidthLfoAmplitude;
            this.SelectedSignalType = data.OsciType;
            this.PusleWidth = data.PusleWidth;
            this.OsciCount = data.OsciCount;
            this.Pitch = data.MultiOsciPitch;
            this.SubOszVolume = data.SubOszVolume;
            this.UseAmplitudeLfo = data.UseAmplitudeLfo;
            this.AmplitudeLfoFrequenc = data.AmplitudeLfoFrequenc;
            this.AmplitudeLfoAmplitude = data.AmplitudeLfoAmplitude;
            this.AmplitudeLfoPulseWidth = data.AmplitudeLfoPulseWidth;
            this.UseAccordEffect = data.UseAccordEffect;
        }
    }
}
