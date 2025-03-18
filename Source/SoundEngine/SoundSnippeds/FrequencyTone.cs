using System;
using WaveMaker;
using WaveMaker.KeyboardComponents;
using WaveMaker.Sequenzer;

namespace SoundEngine.SoundSnippeds
{
    class FrequencyTone : ISingleSampleProvider, ISoundSnipped, IFrequenceToneSnipped
    {
        private PianoSequenzer sequenzer;
        private int keyIndex = -1;
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
            if (this.keyIndex != -1) Stop();

            this.IsRunning = true;
            this.keyIndex = this.sequenzer.StartPlayingKey(this.Frequency);
        }
        public void Stop()
        {
            this.IsRunning = false;
            this.sequenzer.ReleaseKey(this.keyIndex);
            this.keyIndex = -1;                      
        }
        public float Volume { get; set; } = 1;

        private float frequency = 440;
        public float Frequency
        {
            get => this.frequency;
            set
            {
                this.frequency = value;
                if (this.keyIndex != -1) this.sequenzer.SetFrequencyFromPlayingTone(this.keyIndex, this.frequency);
            }
        }
        public float Pitch { get { return this.sequenzer.Synthesizer.AudioFilePitch; } set { this.sequenzer.Synthesizer.AudioFilePitch = value; } } 
        public Synthesizer Synthesizer { get { return this.sequenzer.Synthesizer; } }
    }
}
