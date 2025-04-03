using WaveMaker;
using WaveMaker.KeyboardComponents;
using WaveMaker.Sequenzer;

namespace SoundEngine.SoundSnippeds
{
    class FrequencyTone : ISingleSampleProvider, ISoundSnipped, IFrequenceToneSnipped
    {
        private PianoSequenzer sequenzer;
        private List<int> runningKeys = new List<int>();

        internal FrequencyTone(PianoSequenzer sequenzer)
        {
            this.sequenzer = sequenzer;
            this.Frequency = sequenzer.TestToneFrequence;
        }

        public int SampleRate { get { return this.sequenzer.SampleRate; } }
        public float GetNextSample()
        {
            float f = this.sequenzer.GetNextSample(false, 1) * this.Volume;
            return f;
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
                    this.IsRunningChanged?.Invoke(this.isRunning);
                }

            }
        }
        public Action<bool> IsRunningChanged { get; set; } = null;
        public void Play()
        {
            this.IsRunning = true;

            PlayAndReturnHandle();
        }
        public void Stop()
        {
            this.IsRunning = false;

            foreach (int keyIndex in this.runningKeys.ToList())
            {
                this.sequenzer.ReleaseKey(keyIndex);
            }
            this.runningKeys.Clear();
        }
        public int PlayAndReturnHandle()
        {
            int index = this.sequenzer.StartPlayingKey(this.Frequency);
            if (index != -1)
            {
                this.runningKeys.Add(index);
            }
            return index;
        }
        public void StopFromHandle(int handle)
        {
            if (this.runningKeys.Contains(handle) == false) return;

            this.runningKeys.Remove(handle);
            this.sequenzer.ReleaseKey(handle);

            this.IsRunning = this.runningKeys.Any();
        }
        public float Volume { get; set; } = 1;

        private float frequency = 440;
        public float Frequency
        {
            get => this.frequency;
            set
            {
                this.frequency = value;

                foreach (int keyIndex in this.runningKeys.ToList())
                {
                    this.sequenzer.SetFrequencyFromPlayingTone(keyIndex, this.frequency);
                }
            }
        }
        public float Pitch { get { return this.sequenzer.Synthesizer.AudioFilePitch; } set { this.sequenzer.Synthesizer.AudioFilePitch = value; } } 
        public Synthesizer Synthesizer { get { return this.sequenzer.Synthesizer; } }
    }
}
