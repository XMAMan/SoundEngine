using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using WaveMaker.KeyboardComponents;

namespace MusicMachine.ViewModel.SynthesizerVM
{
    public class OscillatorViewModel : ReactiveObject
    {
        private Synthesizer model;
        public OscillatorViewModel(Synthesizer model)
        {
            this.model = model;
        }

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
            get { return this.model.OsziType; }
            set { this.model.OsziType = value; this.RaisePropertyChanged(nameof(SelectedSignalType)); }
        }

        public float PusleWidth
        {
            get { return this.model.PusleWidth; }
            set { this.model.PusleWidth = value; this.RaisePropertyChanged(nameof(PusleWidth)); }
        }

        public int OsziCount
        {
            get { return this.model.OsziCount; }
            set { this.model.OsziCount = value; this.RaisePropertyChanged(nameof(OsziCount)); }
        }

        public float Pitch
        {
            get { return this.model.Pitch; }
            set { this.model.Pitch = value; this.RaisePropertyChanged(nameof(Pitch)); }
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
        public bool UseAccordEffekt
        {
            get { return this.model.UseAccordEffekt; }
            set { this.model.UseAccordEffekt = value; this.RaisePropertyChanged(nameof(UseAccordEffekt)); }
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
            this.SelectedSignalType = data.OsziType;
            this.PusleWidth = data.PusleWidth;
            this.OsziCount = data.OsziCount;
            this.Pitch = data.Pitch;
            this.SubOszVolume = data.SubOszVolume;
            this.UseAmplitudeLfo = data.UseAmplitudeLfo;
            this.AmplitudeLfoFrequenc = data.AmplitudeLfoFrequenc;
            this.AmplitudeLfoAmplitude = data.AmplitudeLfoAmplitude;
            this.AmplitudeLfoPulseWidth = data.AmplitudeLfoPulseWidth;
            this.UseAccordEffekt = data.UseAccordEffekt;
        }
    }
}
