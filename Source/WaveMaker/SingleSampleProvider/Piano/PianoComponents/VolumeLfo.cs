namespace WaveMaker.KeyboardComponents
{
    public class VolumeLfo : IPianoComponent
    {
        private IPianoComponent source;
        private LowFrequenzyOscillator lfo;

        public bool IsEnabled { get; set; } = false;
        public float Frequency { get => this.lfo.Frequency; set => this.lfo.Frequency = value; }

        public VolumeLfo(IPianoComponent source, int sampleRate) 
        {
            this.source = source;
            this.lfo = new LowFrequenzyOscillator(sampleRate) { SignalType = SignalType.Sinus, PusleWidth = 0.5f };
        }

        public float GetSample(KeySampleData data)
        {
            float inSample = this.source.GetSample(data);
            if (this.IsEnabled == false) return inSample;

            float lfoValue = this.lfo.GetSample(data.SampleIndex);

            //lfoValue = Math.Max(0, lfoValue); //Values 0 or 1

            return inSample * lfoValue;
        }
    }
}
