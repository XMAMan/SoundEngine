using System;
using WaveMaker.BlockSampleEffects;

namespace WaveMaker.KeyboardComponents
{
    public class AudioFile : IPianoComponent
    {
        public int SampleRate { get; private set; }
        public string AudioFileName { get; set; } = "Load Audio-File";

        private float[] modifiedSamples = null;

        private float[] sampleData = null;
        public float[] SampleData
        {
            get { return this.sampleData; }
            set { this.sampleData = value; UpdateModifiedSamples(); }
        }

        private float pitch = 1;
        public float Pitch 
        {
            get { return this.pitch; }
            set { this.pitch = value; UpdateModifiedSamples(); }
        }

        private void UpdateModifiedSamples()
        {
            if (this.SampleData == null) return;
            PitchShifter shifter = new PitchShifter(this.SampleRate) { Pitch = this.Pitch };
            this.modifiedSamples = shifter.GetModifiSamples(this.SampleData);
        }

        //Hiermit kann der Anfang des Audiofiles weggeschnitten werden
        private int leftIndex = 0;
        private float leftPositionInMilliseconds;
        public float LeftPositionInMilliseconds
        {
            get { return this.leftPositionInMilliseconds; }
            set { this.leftPositionInMilliseconds = value; this.leftIndex = (int)(value / 1000 * this.SampleRate); }
        }

        //Hiermit kann das Ende des AudioFiles weggeschnitten werden
        private int rightIndex = 0;
        private float rightPositionInMilliseconds;
        public float RightPositionInMilliseconds
        {
            get { return this.rightPositionInMilliseconds; }
            set { this.rightPositionInMilliseconds = value; this.rightIndex = (int)(value / 1000 * this.SampleRate); }
        }

        private float gainPow = 1;
        private float gain = 1;
        public float Gain //Verstärkungsfaktor (Da meine Aufnahmen so leise sind) (Im Gegensatz zum Volume, was nur von 0 bis 1 geht, geht Gain von 1 bis beliebig Groß. Somit ist Übersteuerung möglich)
        {
            get { return this.gain; }
            set { this.gain = value; this.gainPow = (float)Math.Pow(value, 2); }
        }

        public float GetFileLengthInMilliseconds()
        {
            if (this.modifiedSamples == null) return 0;
            return this.modifiedSamples.Length / (float)this.SampleRate * 1000;
        }

        public AudioFile(int sampleRate)
        {
            this.SampleRate = sampleRate;
        }

        public float GetSample(KeySampleData data)
        {
            return GetSample(data.SampleIndex);
        }

        public float GetSample(int sampleIndex)
        {
            if (this.modifiedSamples == null) return 0;

            if (sampleIndex + this.leftIndex < this.rightIndex)
            {
                float f = this.modifiedSamples[sampleIndex + this.leftIndex] * this.gainPow;
                //f = Limiter(f); //Nur mal kurz zum Test
                if (f > 1) f = 1;
                if (f < -1) f = -1;
                return f;
            }

            return 0;
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
