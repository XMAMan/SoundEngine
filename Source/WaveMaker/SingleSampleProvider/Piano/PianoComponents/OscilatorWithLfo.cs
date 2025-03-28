namespace WaveMaker.KeyboardComponents
{
    public class OscilatorWithLfo : Oscillator
    {
        private LowFrequenzyOszillator lfoFrequency;
        private LowFrequenzyOszillator lfoAmplitude;
        private LowFrequenzyOszillator lfoPulsewidth;
        private Ramp ramp;

        public OscilatorWithLfo(int sampleRate)
            :base(sampleRate)
        {
            this.lfoFrequency = new LowFrequenzyOszillator(sampleRate) { Frequency = 5, Amplitude = 0.8f, SignalType = SignalType.Sinus };
            this.lfoAmplitude = new LowFrequenzyOszillator(sampleRate) { SignalType = SignalType.Rectangle, PusleWidth = 1.0f }; //PulseWidth von 1 bedeutet, es wird eines Sinusschwingung verwendet (Siehe Zeile 42)
            this.lfoPulsewidth = new LowFrequenzyOszillator(sampleRate) { Frequency = 5, Amplitude = 0.2f, SignalType = SignalType.Sinus };
            this.ramp = new Ramp(sampleRate);
        }

        public bool UseFrequencyLfo { get; set; } = false;
        public float FrequencyLfoFrequenc { get { return this.lfoFrequency.Frequency; } set { this.lfoFrequency.Frequency = value; } }
        public float FrequencyLfoAmplitude { get { return this.lfoFrequency.Amplitude; } set { this.lfoFrequency.Amplitude = value; } }

        public bool UseAmplitudeLfo { get; set; } = false;
        public float AmplitudeLfoFrequenc { get { return this.lfoAmplitude.Frequency; } set { this.lfoAmplitude.Frequency = value; } }
        public float AmplitudeLfoAmplitude { get { return this.lfoAmplitude.Amplitude; } set { this.lfoAmplitude.Amplitude = value; } }
        public float AmplitudeLfoPulseWidth { get { return this.lfoAmplitude.PusleWidth; } set { this.lfoAmplitude.PusleWidth = value; } }

        public bool UsePulsewidthLfo { get; set; } = false;
        public float PulsewidthLfoFrequence { get { return this.lfoPulsewidth.Frequency; } set { this.lfoPulsewidth.Frequency = value; } }
        public float PulsewidthLfoAmplitude { get { return this.lfoPulsewidth.Amplitude; } set { this.lfoPulsewidth.Amplitude = value; } }

        public bool UseFrequenceRamp { get; set; } = false;
        public float FrequencyRampFactor { get { return this.ramp.IncreasePerSecond; } set { this.ramp.IncreasePerSecond = value; } }
        public int RampStepFactor { get { return this.ramp.StepFactor; } set { this.ramp.StepFactor = value; } }

        public bool UseAccordEffekt { get; set; } = false;

        public override float GetSampleFromFrequence(int sampleIndex, float frequency, float pulseWidth)
        {
            if (this.lfoAmplitude.PusleWidth == 1) this.lfoAmplitude.SignalType = SignalType.Sinus; else this.lfoAmplitude.SignalType = SignalType.Rectangle;

            float lfoFrequency = this.UseFrequencyLfo ? this.lfoFrequency.GetSample(sampleIndex) : 0;
            float lfoAmplitude = this.UseAmplitudeLfo ? (1 + this.lfoAmplitude.GetSample(sampleIndex)) : 1; //Basisfrequenz ist 1. Um diese 1 herrum moduliert der Amplituden-LFO
            float lfoPulseWidth = this.UsePulsewidthLfo ? (this.lfoPulsewidth.GetSample(sampleIndex) * 0.5f) : 0; 

            
            float modulatedFrequence = frequency + lfoFrequency; //Basisfrequence ist 'frequency'. Um diesen Wert wird moduliert
            //float modulatedFrequence = frequency * (float)Math.Pow(2, lfoFrequency / 12f); //Erhöhere/Veringere Frequenz um 1 Halbtonschritt, wenn die Amplitude vom FrequenceLFO 1 ist
            float modulatedPulse = pulseWidth + lfoPulseWidth; //Basispulse ist 'pulseWidth'

            if (this.UseFrequenceRamp)
            {
                float frequenceFromRamp = Math.Max(1, modulatedFrequence * this.ramp.GetFrequencyFactor(sampleIndex));
                return GetSampleWithAccord(sampleIndex, frequenceFromRamp, modulatedPulse) * lfoAmplitude;
            }

            return GetSampleWithAccord(sampleIndex, modulatedFrequence, modulatedPulse) * lfoAmplitude;
        }

        private float GetSampleWithAccord(int sampleIndex, float frequency, float pulseWidth)
        {
            float sum = base.GetSample(sampleIndex, frequency, pulseWidth);
            if (this.UseAccordEffekt)
            {
                sum += base.GetSample(sampleIndex, frequency * (float)Math.Pow(2, 4 / 12f), pulseWidth);
                sum += base.GetSample(sampleIndex, frequency / (float)Math.Pow(2, 5 / 12f), pulseWidth);
            }
            return sum;
        }
    }
  
    class Ramp
    {
        private int samplesPerSecond;

        public float IncreasePerSecond { get; set; } = -0.3f; //Geht von -8 bis +8
        public int StepFactor { get; set; } = 0; //Geht von 0 bis 100 (0 = Deaktiviert)

        public Ramp(int sampleRate)
        {
            this.samplesPerSecond = sampleRate;
        }

        public float GetFrequencyFactor(int sampleIndex)
        {
            float f = (float)Math.Pow(1 + sampleIndex / (float)this.samplesPerSecond, this.IncreasePerSecond); //Exponentieller Rampe
            if (this.StepFactor > 0)
                f = ((int)(f * this.StepFactor)) / (float)this.StepFactor; //Treppen-Effekt
            return f;
        }
    }

    class LowFrequenzyOszillator
    {
        public float Amplitude { get; set; } = 1;
        public float Frequency { get; set; } = 5;
        public SignalType SignalType { get { return this.oscillator.OsziType; } set { this.oscillator.OsziType = value; } }
        public float PusleWidth { get { return this.oscillator.PusleWidth; } set { this.oscillator.PusleWidth = value; } }

        private Oscillator oscillator;

        public LowFrequenzyOszillator(int sampleRate)
        {
            this.oscillator = new Oscillator(sampleRate);
        }

        public float GetSample(int sampleIndex)
        {
            return this.oscillator.GetSampleFromFrequence(sampleIndex, this.Frequency, this.oscillator.PusleWidth) * this.Amplitude;
        }

    }
}
