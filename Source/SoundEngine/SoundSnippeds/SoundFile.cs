using WaveMaker;
using WaveMaker.KeyboardComponents;

namespace SoundEngine.SoundSnippeds
{
    //wav,wma,mp3,... (Alles was NAudio untersützt)
    class SoundFile : ISingleSampleProvider, ISoundSnipped, IAudioFileSnipped
    {
        private AudioFile audioFile;
        private Synthesizer synthesizer;
        private KeySampleData keySampleData = new KeySampleData(float.NaN);

        internal SoundFile(int sampmleRate, float[] samples)
        {
            this.SampleRate = sampmleRate;

            this.audioFile = GetFileFromSamples(sampmleRate, samples);

            this.synthesizer = new Synthesizer(sampmleRate, null) 
            { 
                SignalSource = SignalSource.AudioFile,
                AudioFileData = samples,
                LeftAudioFilePosition = 0,
                RightAudioFilePosition = audioFile.GetFileLengthInMilliseconds(),
                AttackTimeInMs = 0,
                DecayTimeInMs = 0,
                SustainVolume = 1,
                ReleaseTimeInMs = 0,
            };           
        }

        public IAudioFileSnipped GetCopy()
        {
            var copy = new SoundFile(this.SampleRate, this.audioFile.SampleData.ToArray());
            copy.isRunning = this.isRunning;
            copy.Volume = this.Volume;
            copy.AutoLoop = this.AutoLoop;
            copy.Pitch = this.Pitch;
            copy.Speed = this.Speed;
            copy.UseDelayEffect = this.UseDelayEffect;
            copy.UseHallEffect = this.UseHallEffect;
            copy.UseGainEffect = this.UseGainEffect;
            copy.Gain = this.Gain;
            copy.UseVolumeLfo = this.UseVolumeLfo;
            copy.VolumeLfoFrequency = this.VolumeLfoFrequency;

            this.CopyWasCreated?.Invoke(copy);
            return copy;
        }

        public Action<ISoundSnipped> CopyWasCreated { get; set; } = null;
        public Action<ISoundSnipped> DisposeWasCalled { get; set; } = null;
        public void Dispose()
        {
            this.DisposeWasCalled?.Invoke(this);
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

            this.keySampleData.SampleIndex++;
            if (this.keySampleData.SampleIndex >= (int)(this.audioFile.SampleData.Length * this.Speed))
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
            this.keySampleData.SampleIndex = 0;
        }
        public float Volume { get; set; } = 1;
        public bool AutoLoop { get; set; } = false;

        public float Pitch { get { return this.synthesizer.AudioFilePitch; } set { this.synthesizer.AudioFilePitch = value; } }
        public float Speed { get { return this.synthesizer.AudioFileSpeed; } set { this.synthesizer.AudioFileSpeed = value; } }
        public bool UseDelayEffect { get { return this.synthesizer.UseDelayEffect; } set { this.synthesizer.UseDelayEffect = value; } }
        public bool UseHallEffect { get { return this.synthesizer.UseHallEffect; } set { this.synthesizer.UseHallEffect = value; } }
        public bool UseGainEffect { get { return this.synthesizer.UseGainEffect; } set { this.synthesizer.UseGainEffect = value; } }
        public float Gain { get { return this.synthesizer.Gain; } set { this.synthesizer.Gain = value; } }
        public bool UsePitchEffect { get { return this.synthesizer.UsePitchEffect; } set { this.synthesizer.UsePitchEffect = value; } }
        public float PitchEffect { get { return this.synthesizer.PitchEffect; } set { this.synthesizer.PitchEffect = value; } }
        public bool UseVolumeLfo { get { return this.synthesizer.UseVolumeLfo; } set { this.synthesizer.UseVolumeLfo = value; } }
        public float VolumeLfoFrequency { get { return this.synthesizer.VolumeLfoFrequency; } set { this.synthesizer.VolumeLfoFrequency = value; } }
    }
}
