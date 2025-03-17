using System.Linq;

namespace WaveMaker.KeyboardComponents
{
    //Hall = Mehrfacher Delay-Effekt
    public class HallEffect : IPianoComponent, IPianoStopKeyHandler
    {
        private IPianoComponent source;
        private DelayEffect[] delays;

        public int HallCount { get; set; } = 5; //Anzahl der Schallwiederholungen
        public bool IsEnabled { get; set; } = false;
        public float Gain //geht von 0 bis +1 (Stärke des Effekts)
        { 
            get
            {
                return this.delays[0].Gain;
            }
            set
            {
                foreach (var d in this.delays) d.Gain = value;
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
                foreach (var d in this.delays) d.DelayTimeInMs = value;
            }
        }

        public int StopIndexTime { get { return this.delays[0].StopIndexTime; } }

        public HallEffect(IPianoComponent source, int sampleRate)
        {
            this.source = source;

            this.delays = new DelayEffect[this.HallCount];

            this.delays[0] = new DelayEffect(source, sampleRate) { IsEnabled = true, DelayTimeInMs = 100 };
            for (int i = 1; i < this.delays.Length; i++) this.delays[i] = new DelayEffect(this.delays[i - 1], sampleRate) { IsEnabled = true, DelayTimeInMs = 100 };
        }

        public float GetSample(KeySampleData data)
        {
            float inSample = this.source.GetSample(data);
            if (this.IsEnabled == false) return inSample;

            return this.delays.Last().GetSample(data);
        }
    }
}
