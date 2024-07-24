using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveMaker.KeyboardComponents
{
    public class AdsrEnvelope : IPianoComponent, IPianoStopKeyHandler
    {
        private int samplesPerSecond;
        private IPianoComponent source;
        private int AttackIndexTime { get; set; } = 0;
        private int DecayIndexTime { get; set; } = 0;
        private int ReleaseIndexTime { get; set; } = 0;
        private float attackTimeInMs = 70;
        public float AttackTimeInMs
        {
            get
            {
                return this.attackTimeInMs;
            }
            set
            {
                this.attackTimeInMs = value;
                this.AttackIndexTime = (int)(this.attackTimeInMs * this.samplesPerSecond / 1000);
            }
        }
        private float decayTimeInMs = 50;
        public float DecayTimeInMs
        {
            get
            {
                return this.decayTimeInMs;
            }
            set
            {
                this.decayTimeInMs = value;
                this.DecayIndexTime = (int)(this.decayTimeInMs * this.samplesPerSecond / 1000);
            }
        }

        private float releaseTimeInMs = 250;
        public float ReleaseTimeInMs
        {
            get
            {
                return this.releaseTimeInMs;
            }
            set
            {
                this.releaseTimeInMs = value;
                this.ReleaseIndexTime = (int)(this.releaseTimeInMs * this.samplesPerSecond / 1000);
            }
        }
        public float SustainVolume { get; set; } = 0.9f;

        public AdsrEnvelope(IPianoComponent source, int sampleRate)
        {
            this.source = source;
            this.samplesPerSecond = sampleRate;

            AttackTimeInMs = 50;
            DecayTimeInMs = 70;
            ReleaseTimeInMs = 250;
        }

        //Die Zeitachse der Hüllkurve beginnt bei Index 0.
        public float GetSample(KeySampleData data)
        {
            float volume = 0;
            if (data.SampleIndex < this.AttackIndexTime)
            {
                //Attack-Phase
                volume = 1.0f / this.AttackIndexTime * data.SampleIndex;
            }
            else if (data.SampleIndex >= this.AttackIndexTime && data.SampleIndex < this.AttackIndexTime + this.DecayIndexTime)
            {
                //Decay-Phase
                volume = 1 - (1 - this.SustainVolume) / this.DecayIndexTime * (data.SampleIndex - this.AttackIndexTime);
            }
            else if (data.SampleIndex >= this.AttackIndexTime + this.DecayIndexTime && data.SampleIndex < data.KeyUpSampleIndex)
            {
                //Sustain
                volume = this.SustainVolume;
            }
            else if (data.SampleIndex >= data.KeyUpSampleIndex && data.SampleIndex < data.KeyUpSampleIndex + this.ReleaseIndexTime)
            {
                //Release-Phase
                volume = this.SustainVolume - this.SustainVolume / this.ReleaseIndexTime * (data.SampleIndex - data.KeyUpSampleIndex);
            }

            return this.source.GetSample(data) * volume;
        }

        public bool IsEnabled { get { return true; } }
        public int StopIndexTime { get { return this.ReleaseIndexTime; } }
    }
}
