using SoundEngine.SoundSnippeds;
using WaveMaker;

namespace SoundEngine
{
    public class SoundGenerator : IDisposable, ISoundGenerator
    {
        private IAudioPlayer audioPlayer;
        private IAudioRecorder audioRecorder;
        
        private SoundSnippedCollection sampleProviderCollection;

        public SoundGenerator()
        {
            this.audioRecorder = new NAudioWaveMaker.AudioRecorder(this.SampleRate);
            var audioRecorderSnipp = new AudioRecorderSnipped(this.SampleRate, this.audioRecorder);
            this.sampleProviderCollection = new SoundSnippedCollection(new NAudioWaveMaker.AudioFileHandler(), this.SampleRate, this.audioRecorder);
            this.sampleProviderCollection.AddAudioRecorderSnipped(audioRecorderSnipp);

            this.audioPlayer = new NAudioWaveMaker.AudioPlayer(this.sampleProviderCollection);
            this.audioPlayer.StartPlaying();
            this.audioPlayer.SelectedDevice = this.audioPlayer.GetAvailableDevices()[0]; //Nutze das erste gefundene Audio-Device zur Ausgabe

            this.AudioRecorder = audioRecorderSnipp;
            this.AudioFileWriter = new NAudioWaveMaker.AudioFileHandler();
        }

        public float Volume { get { return this.sampleProviderCollection.Volume; } set { this.sampleProviderCollection.Volume = value; } }
        public string SelectedOutputDevice { get { return this.audioPlayer.SelectedDevice; } set { this.audioPlayer.SelectedDevice = value; } }
        public string[] GetAvailableOutputDevices() { return this.audioPlayer.GetAvailableDevices(); }

        public int SampleRate { get { return 44100 / 2; } }

        public IAudioRecorderSnipped AudioRecorder { get; private set; }
        public IAudioFileWriter AudioFileWriter { get; private set; }

        //Wird zyklisch vom Timer gerufen, wenn er nach neuen Audiodaten fragt.
        //Kann benutzt werden, um Audiodaten aufzuzeichnen oder sie zu visualisieren
        public event EventHandler<float[]> AudioOutputCallback
        {
            add
            {
                if (this.audioPlayer != null)
                {
                    this.audioPlayer.AudioOutputCallback += value;
                }
            }
            remove
            {
                if (this.audioPlayer != null)
                {
                    this.audioPlayer.AudioOutputCallback -= value;
                }
            }
        }

        public void Dispose()
        {
            if (this.audioPlayer is IDisposable)
            {
                (this.audioPlayer as IDisposable).Dispose();
            }

            if (this.audioRecorder is IDisposable)
            {
                (this.audioRecorder as IDisposable).Dispose();
            }
        }

        //wav,wma,mp3,... (Alles was NAudio untersützt)
        public IAudioFileSnipped AddSoundFile(string audioFile)
        {
            return this.sampleProviderCollection.AddSoundFile(audioFile);
        }

        //.music-Dateien wo ich die darin enthaltenen Instrumente einzeln starten/stoppen will
        public IFrequenceToneSnipped[] AddSynthSoundCollection(string musicFile)
        {
            return this.sampleProviderCollection.AddSynthSoundCollection(musicFile);
        }

        //.music-Dateien (Beispiel: Intro-Musik die aus mehreren Instrumenten besteht) -> Die Wiedergabe läßt alle Einzelsequenzer gleichzeitig abspielen
        public IMusicFileSnipped AddMusicFile(string musicFile)
        {
            return this.sampleProviderCollection.AddMusicFile(musicFile);
        }

        //.synt-Datei
        public IFrequenceToneSnipped AddFrequencyTone(string syntiFile)
        {
            return this.sampleProviderCollection.AddFrequencyTone(syntiFile);
        }
    }
}
