using System;
using WaveMaker;
using WaveMaker.KeyboardComponents;

namespace SoundEngine.SoundSnippeds
{
    //wav,wma,mp3,... (Alles was NAudio untersützt)
    class SoundFile : ISingleSampleProvider, ISoundSnipped, IAudioFileSnipped
    {
        private AudioFile audioFile;
        private Synthesizer synthesizer;
        //private int sampleIndex = 0;
        private KeySampleData keySampleData = new KeySampleData(float.NaN);
        internal SoundFile(int sampmleRate, float[] samples)
        {
            this.SampleRate = sampmleRate;

            this.audioFile = GetFileFromSamples(sampmleRate, samples);

            this.synthesizer = new Synthesizer(sampmleRate) 
            { 
                UseDataFromAudioFileInsteadFromOszi = true, 
                AudioFileData = samples,
                LeftAudioFilePosition = 0,
                RightAudioFilePosition = audioFile.GetFileLengthInMilliseconds()
            };           
        }

        private AudioFile GetFileFromSamples(int sampmleRate, float[] samples)
        {
            var audioFile = new AudioFile(sampmleRate);
            audioFile.SampleData = samples;
            audioFile.RightPositionInMilliseconds = audioFile.GetFileLengthInMilliseconds();

            return audioFile;
        }

        public int SampleRate { get; private set; }
        public float GetNextSample()
        {
            if (this.IsRunning == false) return 0;

            //this.sampleIndex++;
            this.keySampleData.SampleIndex++;
            //if (this.sampleIndex >= this.audioFile.SampleData.Length)
            if (this.keySampleData.SampleIndex >= this.audioFile.SampleData.Length)
            {
                if (this.AutoLoop)
                {
                    this.keySampleData.SampleIndex = 0;
                }
                else
                {
                    this.IsRunning = false;
                }

                if (this.EndTrigger != null) this.EndTrigger();
            }

            //return this.audioFile.GetSample(this.sampleIndex) * this.Volume;
            return this.synthesizer.GetSample(this.keySampleData) * this.Volume;
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
            Reset();
            this.IsRunning = true;
        }
        public void Stop()
        {
            this.IsRunning = false;
        }
        public void Reset() //Springe zum Anfang zurück
        {
            //this.sampleIndex = 0;
            this.keySampleData.SampleIndex = 0;
        }
        public float Volume { get; set; } = 1;
        public bool AutoLoop { get; set; } = false;

        public float Pitch { get { return this.audioFile.Pitch; } set { this.audioFile.Pitch = value; } }
        public float Speed { get { return this.audioFile.Speed; } set { this.audioFile.Speed = value; } }
        public bool UseDelayEffekt { get { return this.synthesizer.UseDelayEffekt; } set { this.synthesizer.UseDelayEffekt = value; } }
        public bool UseHallEffekt { get { return this.synthesizer.UseHallEffekt; } set { this.synthesizer.UseHallEffekt = value; } }
        public bool UseGainEffekt { get { return this.synthesizer.UseGainEffekt; } set { this.synthesizer.UseGainEffekt = value; } }
        public float Gain { get { return this.synthesizer.Gain; } set { this.synthesizer.Gain = value; } }
        public bool UseVolumeLfo { get { return this.synthesizer.UseVolumeLfo; } set { this.synthesizer.UseVolumeLfo = value; } }
        public float VolumeLfoFrequency { get { return this.synthesizer.VolumeLfoFrequency; } set { this.synthesizer.VolumeLfoFrequency = value; } }
    }
}
