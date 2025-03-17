namespace WaveMaker.KeyboardComponents
{
    public class DelayEffect : IPianoComponent, IPianoStopKeyHandler
    {
        private int samplesPerSecond;
        private IPianoComponent source;
        private float[] buffer;

        public bool IsEnabled { get; set; } = false;
        public float Gain { get; set; } = 0.3f; //geht von 0 bis +1 (Stärke des Effekts)
        private float delayTimeInMs = 200; //Geht von 0 bis MaxAllowedDelayTimeInMs
        public float DelayTimeInMs
        {
            get
            {
                return this.delayTimeInMs;
            }
            set
            {
                this.delayTimeInMs = value;
                this.buffer = new float[(int)(this.samplesPerSecond * this.delayTimeInMs / 1000)];
            }
        }
        //public float MaxAllowedDelayTimeInMs { get; private set; } = 1000;

        public int StopIndexTime { get { return this.buffer.Length; } }

        public DelayEffect(IPianoComponent source, int sampleRate)
        {
            this.source = source;
            this.samplesPerSecond = sampleRate;

            //int maxAllowedLength = (int)(samplesPerSecond * this.MaxAllowedDelayTimeInMs / 1000);
            this.buffer = new float[(int)(this.samplesPerSecond * this.DelayTimeInMs / 1000)];
        }

        public float GetSample(KeySampleData data)
        {
            float inSample = this.source.GetSample(data);
            if (this.IsEnabled == false) return inSample;

            int index = data.SampleIndex % this.buffer.Length;
            float oldSample = this.buffer[index];               //Read Old Sample
            this.buffer[index] = inSample;                      //Write New Sample
            if (data.SampleIndex < this.buffer.Length) return inSample;
            
            return inSample * (1 - this.Gain) + oldSample * this.Gain;
        }
    }
}
