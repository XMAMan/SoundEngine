using WaveMaker;
using WaveMaker.KeyboardComponents;

namespace SoundEngine.SoundSnippeds
{
    //Wiedergabe der Mikrofon-Eingangsdaten aber mit Effekten versehen
    internal class AudioRecorderSnipped : ISingleSampleProvider, IAudioRecorderSnipped
    {
        private IAudioRecorder audioRecorder;
        private Synthesizer synthesizer;
        private KeySampleData keySampleData = new KeySampleData(float.NaN);

        public AudioRecorderSnipped(int sampleRate, IAudioRecorder audioRecorder)
        {
            this.audioRecorder = audioRecorder;
            this.SampleRate = sampleRate;

            this.synthesizer = new Synthesizer(sampleRate, audioRecorder)
            {
                SignalSource = SignalSource.Microphone,
            };
        }

        public IAudioRecorderSnipped GetCopy()
        {
            var copy = new AudioRecorderSnipped(this.SampleRate, this.audioRecorder);
            this.CopyWasCreated?.Invoke(copy);
            return copy;
        }
        public Action<ISoundSnipped> CopyWasCreated { get; set; } = null;
        public Action<ISoundSnipped> DisposeWasCalled { get; set; } = null;
        public void Dispose()
        {
            this.DisposeWasCalled?.Invoke(this);
        }

        public int SampleRate { get; private set; }

        public float GetNextSample()
        {
            if (this.IsRunning == false) return 0;

            this.keySampleData.SampleIndex++;

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

        public void Play()
        {
            this.audioRecorder.StartRecording();
            this.IsRunning = true;
        }
        public void Stop()
        {
            this.IsRunning = false;
            this.audioRecorder.StopRecording();            
        }

        public float Volume { get; set; } = 1;

        public bool UseDelayEffect { get { return this.synthesizer.UseDelayEffect; } set { this.synthesizer.UseDelayEffect = value; } }
        public bool UseHallEffect { get { return this.synthesizer.UseHallEffect; } set { this.synthesizer.UseHallEffect = value; } }
        public bool UseGainEffect { get { return this.synthesizer.UseGainEffect; } set { this.synthesizer.UseGainEffect = value; } }
        public float Gain { get { return this.synthesizer.Gain; } set { this.synthesizer.Gain = value; } }
        public bool UsePitchEffect { get { return this.synthesizer.UsePitchEffect; } set { this.synthesizer.UsePitchEffect = value; } }
        public float PitchEffect { get { return this.synthesizer.PitchEffect; } set { this.synthesizer.PitchEffect = value; } }
        public bool UseVolumeLfo { get { return this.synthesizer.UseVolumeLfo; } set { this.synthesizer.UseVolumeLfo = value; } }
        public float VolumeLfoFrequency { get { return this.synthesizer.VolumeLfoFrequency; } set { this.synthesizer.VolumeLfoFrequency = value; } }

        public string[] GetAvailableDevices()
        {
            return this.audioRecorder.GetAvailableDevices();
        }
        public string SelectedDevice { get { return this.audioRecorder.SelectedDevice; } set { this.audioRecorder.SelectedDevice = value; } }
        public void UseDefaultDevice()
        {
            this.audioRecorder.UseDefaultDevice();
        }
    }
}
