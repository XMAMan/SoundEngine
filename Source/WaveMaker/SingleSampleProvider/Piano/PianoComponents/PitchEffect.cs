using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveMaker.BlockSampleEffects;
using WaveMaker.KeyboardComponents;

namespace WaveMaker.SingleSampleProvider.Piano.PianoComponents
{
    //Erzeugt eine Latenz von 4096 Samples, da der Effekt blockweise arbeitet
    public class PitchEffect : IPianoComponent
    {
        private IPianoComponent source;
        private PitchShifter shifter;
        private float[] ringBuffer;
        private int ringPosition = 0;

        private float[] modifiedSamples = null;

        //sorge dafür, dass wenn GetSample für ein SampleIndex mehrmals aufgerufen wird, dass es immer den gleichen Wert zurück gibt
        private int alreadyUsedIndex = -1;
        private float lastSample = 0;

        public bool IsEnabled { get; set; } = false;
        public float Pitch { get => this.shifter.Pitch; set => this.shifter.Pitch = value; } //0..2 (1 = no change)

        public PitchEffect(IPianoComponent source, int sampleRate)
        {
            this.source = source;
            this.shifter = new PitchShifter(sampleRate);
            this.ringBuffer = new float[this.shifter.FrameSize];
        }

        public float GetSample(KeySampleData data)
        {
            float inSample = this.source.GetSample(data);
            if (this.IsEnabled == false) return inSample;

            if (data.SampleIndex != this.alreadyUsedIndex)
            {
                this.alreadyUsedIndex = data.SampleIndex;
                this.lastSample = GetModifiedSample(inSample);
                return this.lastSample;
            }
            else
            {
                return this.lastSample;
            }
        }

        private float GetModifiedSample(float inSample)
        {
            this.ringBuffer[this.ringPosition] = inSample;
            this.ringPosition++;
            if (this.ringPosition >= this.shifter.FrameSize)
            {
                this.ringPosition = 0;
                this.modifiedSamples = this.shifter.GetModifiSamples(this.ringBuffer);
            }

            if (this.modifiedSamples == null) return inSample; //Wenn der erste Datenblock noch nicht voll ist gebe den Originalwert zurück

            float outSample = this.modifiedSamples[this.ringPosition];

            return outSample;
        }
    }
}
