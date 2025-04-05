namespace WaveMaker.KeyboardComponents
{
    //Weiche, welche zwischen Oszi, AudioFile und Mikrofon umschalten kann
    public class Switch : IPianoComponent
    {
        private SignalSource signalSource = SignalSource.Oscillator;
        public SignalSource SignalSource
        {
            get => this.signalSource;
            set
            {
                this.signalSource = value;

                switch (value)
                {
                    case SignalSource.Oscillator:
                        this.selectedComponent = this.osci;
                        break;

                    case SignalSource.AudioFile:
                        this.selectedComponent = this.audioFile;
                        break;

                    case SignalSource.Microphone:
                        this.selectedComponent = this.microphone;
                        break;
                }
            }
        }

        private IPianoComponent osci, audioFile, microphone;
        private IPianoComponent selectedComponent = null;

        public Switch(IPianoComponent osci, IPianoComponent audioFile, IPianoComponent microphone)
        {
            this.osci = osci;
            this.audioFile = audioFile;
            this.microphone = microphone;

            this.SignalSource = SignalSource.Oscillator;
        }

        public float GetSample(KeySampleData data)
        {
            return this.selectedComponent.GetSample(data);
        }
    }
}
