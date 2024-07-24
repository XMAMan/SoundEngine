using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveMaker.KeyboardComponents
{
    public enum SignalType { Sinus, Rectangle, Triangle, SawTooth, Noise }
    public class Oscillator : IPianoComponent
    {
        protected int samplesPerSecond;

        //Für das Noise-Erzeugung
        
        private Filter noiseFilter;

        public float PusleWidth { get; set; } = 0.5f;
        
        public SignalType OsziType { get; set; } = SignalType.Rectangle;

        class NoiseGenerator : IPianoComponent
        {
            private Random rand = new Random(0);

            public float GetSample(KeySampleData data)
            {
                return (float)this.rand.NextDouble() * 2 - 1;
            }
        }

        public Oscillator(int sampleRate)
        {
            this.samplesPerSecond = sampleRate;
            this.noiseFilter = new Filter(new NoiseGenerator(), FilterType.LowPass, sampleRate) { IsEnabled = true, Resonance = 0.9f };
        }
        public virtual float GetSample(KeySampleData data)
        {
            return GetSample(data.SampleIndex, data.Frequency,this.PusleWidth);
        }

        public virtual float GetSampleFromFrequence(int sampleIndex, float frequency, float pulseWidth)
        {
            return GetSample(sampleIndex, frequency, pulseWidth);
        }

        protected float GetSample(int sampleIndex, float frequency, float pulseWidth)
        {
            switch (this.OsziType)
            {
                case SignalType.Sinus:
                    return GetSinusSignal(sampleIndex, frequency);
                case SignalType.Rectangle:
                    return GetSquareSignal(sampleIndex, frequency, pulseWidth);
                case SignalType.Triangle:
                    return GetTriangleSignal(sampleIndex, frequency);
                case SignalType.SawTooth:
                    return GetSawToothSignal(sampleIndex, frequency);
                case SignalType.Noise: //Für Schlaginstrumente (Percussions)
                    return GetColoredNoise(sampleIndex, frequency);
            }

            return float.NaN;
        }


        
        private float GetSinusSignal(int sampleIndex, float frequence)
        {
            return (float)Math.Sin(sampleIndex * Math.PI * 2 * frequence / this.samplesPerSecond);
        }

        private float GetSquareSignal(int sampleIndex, float frequence, float pulseWidth)
        {
            if (((sampleIndex * frequence / this.samplesPerSecond) % 1) > pulseWidth)
            //if (((sampleIndex * frequence / this.samplesPerSecond) % 1) > this.PusleWidth)
                return 1;
            else
                return -1;
            //return ((this.sampleData.CurrentSampleIndex * 2 * frequence / this.sampleData.SamplesPerSecond + this.Phasenshift) % 2) - 1 > 0 ? 1 : -1;
        }

        private float GetTriangleSignal(int sampleIndex, float frequence)
        {
            float multiple = 2 * frequence / this.samplesPerSecond;
            float sampleSaw = ((sampleIndex * multiple) % 2);
            float triangleValue = 2 * sampleSaw;
            if (triangleValue > 1)
                triangleValue = 2 - triangleValue;
            if (triangleValue < -1)
                triangleValue = -2 - triangleValue;

            return triangleValue;
        }

        private float GetSawToothSignal(int sampleIndex, float frequence)
        {
            return ((sampleIndex * 2 * frequence / this.samplesPerSecond) % 2) - 1;
        }

        //https://www.researchgate.net/post/How_we_can_make_sure_that_colored_noise_generation_by_passing_white_Gaussian_noise_from_a_low_pass_filter_is_actual_pink_colored_noise
        //White Noise = Enthält alle Frequenzen
        //Colored Noise = Enthält nicht alle Frequenzen (Signal wurde durch ein Filter gejagt)
        private float GetColoredNoise(int sampleIndex, float frequence)
        {
            this.noiseFilter.CutOffFrequence = Math.Min(1000, frequence) / 1000.0f;
            return this.noiseFilter.GetSample(new KeySampleData(frequence));
        }

    }
}
