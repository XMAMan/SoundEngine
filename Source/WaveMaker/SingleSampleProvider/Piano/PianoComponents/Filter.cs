using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveMaker.KeyboardComponents
{
    public enum FilterType { LowPass, HighPass }
    public class Filter : IPianoComponent
    {
        private int samplesPerSecond;
        private IPianoComponent source;
        private FilterType filterType;

        //Filterkoeffizienten vom IIR-Filter
        private double a0;
        private double a1;
        private double a2;
        private double b1;
        private double b2;

        // state (die letzten Input- Output-Samples)
        private double x1 = 0;
        private double x2 = 0;
        private double y1 = 0;
        private double y2 = 0;

        public Filter(IPianoComponent source, FilterType filterType, int sampleRate)
        {
            this.source = source;
            this.filterType = filterType;
            this.samplesPerSecond = sampleRate;
            this.CutOffFrequence = 1;
        }
        public float GetSample(KeySampleData data)
        {
            float inSample = this.source.GetSample(data);
            if (this.IsEnabled == false) return inSample;

            var result = a0 * inSample + a1 * x1 + a2 * x2 - b1 * y1 - b2 * y2;

            // shift x1 to x2, sample to x1 
            x2 = x1;
            x1 = inSample;

            // shift y1 to y2, result to y1 
            y2 = y1;
            y1 = result;

            return (float)y1;
        }

        private float cutoff = float.NaN;
        public float CutOffFrequence //Geht von 0 bis 1
        {
            get
            {
                return this.cutoff;
            }
            set
            {
                this.cutoff = value;
                CalculateIIRFilterCoeffizients();
            }
        }

        private float resonance = 1.2f; //Verstärkungsfaktor nahe dem Cutoff. Zahl muss zwischen 1 und Wert > 1 liegen
        public float Resonance //Geht von 0 bis float.Max
        {
            get
            {
                return this.resonance - 1;
            }
            set
            {
                this.resonance = Math.Max(value + 1, 1);
                CalculateIIRFilterCoeffizients();
            }
        }

        public bool IsEnabled { get; set; } = false;

        private void CalculateIIRFilterCoeffizients()
        {
            float cutoff1 = this.cutoff * this.cutoff * this.cutoff * this.cutoff; //Ich nehme das hier hoch 4, weil man ansonsten den CUT-Regler ganz nahe zur 0 Regeln muss, um den Tiefpass-Effekt so richtig zu hören

            double theta =  Math.PI * cutoff1;
            double d = 0.5 * (1 / (this.resonance / Math.Sqrt(2))) * Math.Sin(theta);
            double beta = 0.5 * ((1 - d) / (1 + d));
            double gamma = (0.5 + beta) * Math.Cos(theta);

            if (this.filterType == FilterType.LowPass)
            {
                this.a0 = 0.5 * (0.5 + beta - gamma);
                this.a1 = 0.5 + beta - gamma;
            }
            else
            {
                a0 = 0.5 * (0.5 + beta + gamma);
                a1 = -(0.5 + beta + gamma);
            }
            this.a2 = this.a0;
            this.b1 = -2 * gamma;
            this.b2 = 2 * beta;
        }
    }
}
