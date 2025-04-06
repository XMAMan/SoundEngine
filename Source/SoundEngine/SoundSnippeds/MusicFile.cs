using WaveMaker;
using WaveMaker.KeyboardComponents;
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
            bool isRunningOld = this.multiSequenzer.IsRunning && this.multiSequenzer.IsFinish == false;
            float sample = this.multiSequenzer.GetNextSample();

            bool isRunningNext = this.multiSequenzer.IsRunning && this.multiSequenzer.IsFinish == false;

            if (this.IsRunning == false && isRunningNext)
            {
                this.IsRunning = true;
            }

            if (this.IsRunning == true && isRunningNext == false && this.AutoLoop == false)
            {
                this.IsRunning = false;
            }

            if (this.EndTrigger != null && isRunningOld && isRunningNext == false)
            {
                this.EndTrigger();
            }

            return sample;
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
        public Action EndTrigger { get; set; } = null;

        public void Play()
        {
            if (this.multiSequenzer.IsFinish)
            {
                Reset();
            }
            this.multiSequenzer.IsRunning = true;
            this.IsRunning = this.multiSequenzer.IsFinish == false;
        }
        public void Stop()
        {
            this.multiSequenzer.IsRunning = false;
            this.IsRunning = false;
        }
        public void Reset() //Springe zum Anfang zurück
        {
            this.multiSequenzer.CurrentPosition = 0;
        }
        public float Volume { get { return this.multiSequenzer.Volume; } set { this.multiSequenzer.Volume = value; } }
        public bool AutoLoop { get { return this.multiSequenzer.AutoLoop; } set { this.multiSequenzer.AutoLoop = value; } }
        public float KeyStrokeSpeed { get { return this.multiSequenzer.KeyStrokeSpeed; } set { this.multiSequenzer.KeyStrokeSpeed = value; } }
        public int KeyShift { get { return this.multiSequenzer.GetKeyShiftFromFirstSequenzer(); } set { this.multiSequenzer.SetKeyShiftFromAllSequenzer(value); } }

        public Synthesizer GetSynthesizer(int index)
        {
            return this.multiSequenzer.GetAllSequenzers().ToArray()[index].Synthesizer;
        }
        public IMusicFileSnipped GetCopy()
        {
            var copy = new MusicFile(this.multiSequenzer.GetCopy());
            copy.isRunning = this.isRunning;

            this.CopyWasCreated?.Invoke(copy);
            return copy;
        }
        public Action<ISoundSnipped> CopyWasCreated { get; set; } = null;
        public Action<ISoundSnipped> DisposeWasCalled { get; set; } = null;
        public void Dispose()
        {
            this.DisposeWasCalled?.Invoke(this);
        }
    }
}
