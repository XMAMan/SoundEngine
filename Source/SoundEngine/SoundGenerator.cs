using SoundEngine.SoundSnippeds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveMaker;
using WaveMaker.Helper;
using WaveMaker.KeyboardComponents;
using WaveMaker.Sequenzer;

namespace SoundEngine
{
    public class SoundGenerator : IDisposable, ISoundGenerator
    {
        private IWaveMaker waveMaker;
        private SoundSnippedCollection sampleProviderCollection;

        public SoundGenerator()
        {
            this.sampleProviderCollection = new SoundSnippedCollection(new NAudioWaveMaker.AudioFileHandler(), this.SampleRate);

            this.waveMaker = new NAudioWaveMaker.WaveMaker(this.sampleProviderCollection);
            this.waveMaker.StartPlaying();
        }

        public float Volume { get { return this.sampleProviderCollection.Volume; } set { this.sampleProviderCollection.Volume = value; } }

        public int SampleRate { get { return 44100 / 2; } }

        public void Dispose()
        {
            if (this.waveMaker is IDisposable)
            {
                (this.waveMaker as IDisposable).Dispose();
            }
        }

        public IFrequenceToneSnipped[] GetAllFrequenceTones()
        {
            return this.sampleProviderCollection.GetAllFrequenceTones();
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
        public IAudioFileSnipped AddMusicFile(string musicFile)
        {
            return this.sampleProviderCollection.AddMusicFile(musicFile);
        }

        public IFrequenceToneSnipped AddFrequencyTone(string syntiFile)
        {
            return this.sampleProviderCollection.AddFrequencyTone(syntiFile);
        }

        public IFrequenceToneSnipped AddFrequencyTone()
        {
            return this.sampleProviderCollection.AddFrequencyTone();
        }
    }
}
