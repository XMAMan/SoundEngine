using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveMaker.KeyboardComponents
{
    public class GainEffect : IPianoComponent
    {
        private IPianoComponent source;

        public GainEffect(IPianoComponent source)
        {
            this.source = source;
        }

        public bool IsEnabled { get; set; } = false;

        private float gainPow = 1;
        private float gain = 1;
        public float Gain //Verstärkungsfaktor (Da meine Aufnahmen so leise sind) (Im Gegensatz zum Volume, was nur von 0 bis 1 geht, geht Gain von 1 bis beliebig Groß. Somit ist Übersteuerung möglich)
        {
            get { return this.gain; }
            set { this.gain = value; this.gainPow = (float)Math.Pow(value, 2); }
        }

        public float GetSample(KeySampleData data)
        {
            float inSample = this.source.GetSample(data);
            if (this.IsEnabled == false) return inSample;

            float f = inSample * this.gainPow;
            //f = Limiter(f); //Nur mal kurz zum Test
            if (f > 1) f = 1;
            if (f < -1) f = -1;
            return f;
        }

        //Quelle: https://github.com/echonest/remix/blob/master/external/pydirac225/source/Dirac_LE.cpp
        //Übersetzt von Freefall: https://github.com/Freefall63/NAudio-Pitchshifter
        //Limiter constants
        const float LIM_THRESH = 0.95f;
        const float LIM_RANGE = (1f - LIM_THRESH);
        const float M_PI_2 = (float)(Math.PI / 2);
        private float Limiter(float Sample)
        {
            float res = 0f;
            if ((LIM_THRESH < Sample))
            {
                res = (Sample - LIM_THRESH) / LIM_RANGE;
                res = (float)((Math.Atan(res) / M_PI_2) * LIM_RANGE + LIM_THRESH);
            }
            else if ((Sample < -LIM_THRESH))
            {
                res = -(Sample + LIM_THRESH) / LIM_RANGE;
                res = -(float)((Math.Atan(res) / M_PI_2) * LIM_RANGE + LIM_THRESH);
            }
            else
            {
                res = Sample;
            }
            return res;
        }
    }
}
