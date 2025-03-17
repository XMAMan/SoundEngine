using System;
using WaveMaker;
using WaveMaker.KeyboardComponents;

namespace SoundEngine.SoundSnippeds
{
    //wav,wma,mp3,... (Alles was NAudio untersützt)
    class SoundFile : ISingleSampleProvider, ISoundSnipped, IAudioFileSnipped
    {
        private AudioFile audioFile;
        private int sampleIndex = 0;
        internal SoundFile(int sampmleRate, float[] samples)
        {
            this.SampleRate = sampmleRate;
            this.audioFile = new AudioFile(sampmleRate);
            this.audioFile.SampleData = samples;
            this.audioFile.RightPositionInMilliseconds = this.audioFile.GetFileLengthInMilliseconds();
            this.audioFile.Gain = 7; //Lautstärke Extra
        }

        public int SampleRate { get; private set; }
        public float GetNextSample()
        {
            if (this.IsRunning == false) return 0;

            this.sampleIndex++;
            if (this.sampleIndex >= this.audioFile.SampleData.Length)
            {
                if (this.AutoLoop)
                {
                    this.sampleIndex = 0;
                }
                else
                {
                    this.IsRunning = false;
                }
            }

            return this.audioFile.GetSample(this.sampleIndex) * this.Volume;
        }
        private bool isRunning = false;
        public bool IsRunning
        {
            get { return this.isRunning; }
            private set
            {
                if (value != this.isRunning)
                {
                    this.isRunning = value;
                    if (this.IsRunningChanged != null) this.IsRunningChanged(this.isRunning);
                }

            }
        }
        public Action<bool> IsRunningChanged { get; set; } = null;

        public void Play()
        {
            Reset();
            this.IsRunning = true;
        }
        public void Stop()
        {
            this.IsRunning = false;
        }
        public void Reset() //Springe zum Anfang zurück
        {
            this.sampleIndex = 0;
        }
        public float Volume { get; set; } = 1;
        public bool AutoLoop { get; set; } = false;
    }
}
