namespace WaveMaker.KeyboardComponents
{
    //Hall = Mehrfacher Delay-Effekt
    public class HallEffect : IPianoComponent, IPianoStopKeyHandler
    {
        private IPianoComponent source;
        private DelayEffect[] delays;
        private int sampleRate;

        //Anzahl der Schallwiederholungen
        private int hallCount = 5;
        public int HallCount 
        { 
            get => this.hallCount;
            set
            {
                this.hallCount = value;
                SetAllInternSettings(this.sampleRate, value, this.Gain, this.DelayTimeInMs);
            }
        }
        public bool IsEnabled { get; set; } = false;
        public float Gain //geht von 0 bis +1 (Stärke des Effekts)
        { 
            get
            {
                return this.delays[0].Gain;
            }
            set
            {
                for (int i = 0; i < this.delays.Length; i++)
                {
                    this.delays[i].DelayTimeInMs = value - i * 0.05f;
                }
            }
        }

        public float DelayTimeInMs //Geht von 0 bis MaxAllowedDelayTimeInMs
        {
            get
            {
                return this.delays[0].DelayTimeInMs;
            }
            set
            {
                for (int i = 0; i < this.delays.Length; i++)
                {
                    this.delays[i].DelayTimeInMs = value + 300 * i;
                }
            }
        }

        public int StopIndexTime { get { return this.delays[0].StopIndexTime; } }

        public HallEffect(IPianoComponent source, int sampleRate)
        {
            this.source = source;
            this.sampleRate = sampleRate;

            SetAllInternSettings(sampleRate, this.HallCount, 0.3f, 200);
        }

        private void SetAllInternSettings(int sampleRate, int HallCount, float gain, float delayTimeInMs)
        {
            this.delays = new DelayEffect[HallCount];

            this.delays[0] = new DelayEffect(source, sampleRate) { IsEnabled = true };
            for (int i = 1; i < this.delays.Length; i++) this.delays[i] = new DelayEffect(this.delays[i - 1], sampleRate) { IsEnabled = true };

            this.Gain = gain;
            this.DelayTimeInMs = delayTimeInMs;
        }

        public float GetSample(KeySampleData data)
        {
            if (this.IsEnabled == false) return this.source.GetSample(data);

            double sum = 0;
            foreach (var d in this.delays) sum += d.GetSample(data);

            return (float)(sum / this.HallCount);
        }
    }
}
