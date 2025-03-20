using System;
using WaveMaker.KeyboardComponents;

namespace WaveMaker
{
    public class Piano
    {
        public static readonly int NoMoreKeys = -1;

        public Synthesizer Synthesizer { get; private set; }
        private KeyboardKey[] keys = new KeyboardKey[100];

        public int SampleRate { get; private set; }

        public Piano(int sampleRate, IAudioRecorder audioRecorder)
        {
            this.SampleRate = sampleRate;
            this.Synthesizer = new Synthesizer(sampleRate, audioRecorder);
        }
        public float GetNextSample()
        {
            float sum = 0;
            for (int i=0;i<this.keys.Length;i++)
            {
                if (this.keys[i] != null)
                {
                    sum += this.keys[i].GetNextSample();
                    if (keys[i].KeyIsFinished)
                    {
                        this.keys[i] = null;
                    }
                }
            }

            return sum;
        }

        public int StartPlayingKey(int toneIndex)
        {
            return StartPlayingKey(GetFrequencyFromToneIndex(toneIndex));
        }

        public int StartPlayingKey(float frequence)
        {
            for (int i = 0; i < this.keys.Length; i++)
            {
                if (this.keys[i] == null)
                {
                    float frequency = frequence;
                    this.keys[i] = new KeyboardKey(this.Synthesizer, frequency);
                    return i;
                }
            }

            return NoMoreKeys;
            //throw new Exception("Can not press anymore keys");
        }

        public void ReleaseKey(int keyIndex)
        {
            if (keyIndex == NoMoreKeys) return;
            if (this.keys[keyIndex] != null) this.keys[keyIndex].SetKeyUpSampleIndex(this.Synthesizer.GetMaxStopIndexTime());
        }

        public void ReleaseAllKeys()
        {
            for (int i=0;i<this.keys.Length;i++)
            {
                //if (this.keys[i] != null) ReleaseKey(i);
                this.keys[i] = null;
            }
        }

        public void SetToneIndexFromPlayingTone(int keyIndex, int toneIndex)
        {
            SetFrequencyFromPlayingTone(keyIndex, GetFrequencyFromToneIndex(toneIndex));
        }
        public void SetFrequencyFromPlayingTone(int keyIndex, float frequency)
        {
            if (this.keys[keyIndex] != null) this.keys[keyIndex].SetFrequency(frequency);
        }

        //toneIndex: 60 = Middle C; 69 = A (440 Herz)
        private float GetFrequencyFromToneIndex(int toneIndex)
        {
            //Um den nächsten Halbton nach einer Note zu berechnen, muss ich mit 2^(1/12) multiplizieren. 
            //Mach ich das 12 mal, habe ich eine Oktave erhöhe und somit die Frequenz verdoppelt
            int toneAIndex = 9 + 5 * 12; //5 = Das fünfte A (Dieses A hat 440 Herz)
            double frequency = 440 * Math.Pow(2, (toneIndex - toneAIndex) / 12.0);
            return (float)frequency;
        }
    }

    class KeyboardKey
    {
        private IPianoComponent sampleGenerator;
        private KeySampleData sampleData;
        private int keyIsFinishIndex = int.MaxValue;

        public KeyboardKey(IPianoComponent sampleGenerator, float frequency)
        {
            this.sampleGenerator = sampleGenerator;
            this.sampleData = new KeySampleData(frequency); 
        }

        public float GetNextSample()
        {           
            float sample = this.sampleGenerator.GetSample(this.sampleData);
            this.sampleData.SampleIndex++;
            this.KeyIsFinished = this.sampleData.SampleIndex > this.keyIsFinishIndex;
            return sample;
        }

        public void SetKeyUpSampleIndex(int maxStopIndexTime)
        {
            this.sampleData.KeyUpSampleIndex = this.sampleData.SampleIndex;
            this.keyIsFinishIndex = this.sampleData.SampleIndex + maxStopIndexTime;            
        }

        public bool KeyIsFinished { get; private set; } = false;

        public void SetFrequency(float frequency)
        {
            this.sampleData.Frequency = frequency;
        }
    }
}
