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
                return this.modifiedSamples[sampleIndex + this.leftIndex];
            }

            return 0;
        }
    }
}
