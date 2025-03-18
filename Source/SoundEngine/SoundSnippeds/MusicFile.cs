using System;
using WaveMaker;
using WaveMaker.Sequenzer;

namespace SoundEngine.SoundSnippeds
{
    //.music-Dateien
    class MusicFile : ISingleSampleProvider, ISoundSnipped, IMusicFileSnipped
    {
        private MultiSequenzer multiSequenzer;
        internal MusicFile(MultiSequenzer multiSequenzer)
        {
            this.multiSequenzer = multiSequenzer;
        }
        public int SampleRate { get { return this.multiSequenzer.SampleRate; } }
        public float GetNextSample()
        {
            this.IsRunning = this.multiSequenzer.IsRunning && this.multiSequenzer.IsFinish == false;
            return this.multiSequenzer.GetNextSample();
        }
        //public bool IsRunning { get { return this.multiSequenzer.IsRunning; } set { this.multiSequenzer.IsRunning = value; } }

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
            this.multiSequenzer.IsRunning = true;
        }
        public void Stop()
        {
            this.multiSequenzer.IsRunning = false;
        }
        public void Reset() //Springe zum Anfang zurück
        {
            this.multiSequenzer.CurrentPosition = 0;
        }
        public float Volume { get { return this.multiSequenzer.Volume; } set { this.multiSequenzer.Volume = value; } }
        public bool AutoLoop { get { return this.multiSequenzer.AutoLoop; } set { this.multiSequenzer.AutoLoop = value; } }
    }
}
