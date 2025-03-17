using SoundEngine.SoundSnippeds;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WaveMaker;
using WaveMaker.Helper;
using WaveMaker.KeyboardComponents;
using WaveMaker.Sequenzer;

namespace SoundEngine
{
    class SoundSnippedCollection : ISingleSampleProvider
    {
        private List<ISingleSampleProvider> sampleProviders = new List<ISingleSampleProvider>();
        private MultiSequenzer frequenzTones; //Jeder Sequenzer erzeugt ein anderen Ton (Enthält keine Noten; Es wird nur der Synthesizer benutzt)

        private IAudioFileReader audioFileReader;

        private List<FrequencyTone> frequenzeToneList = new List<FrequencyTone>();
        public IFrequenceToneSnipped[] GetAllFrequenceTones()
        {
            return this.frequenzeToneList.ToArray();
        }

        public SoundSnippedCollection(IAudioFileReader audioFileReader, int sampleRate)
        {
            this.SampleRate = sampleRate;
            this.audioFileReader = audioFileReader;

            this.frequenzTones = new MultiSequenzer(this.SampleRate);

            this.frequenzTones.Volume = 1;
        }

        public float Volume { get; set; } = 1;

        public int SampleRate { get; private set; }
        public float GetNextSample()
        {
            float sum = 0;
            foreach (var prov in this.sampleProviders.ToList())
            {
                sum += prov.GetNextSample();
            }
            return sum * this.Volume;
        }

        //wav,wma,mp3,... (Alles was NAudio untersützt)
        public IAudioFileSnipped AddSoundFile(string audioFile)
        {
            float[] samples = this.audioFileReader.GetSamplesFromAudioFile(audioFile, this.SampleRate);
            var soundFile = new SoundFile(this.SampleRate, samples);
            this.sampleProviders.Add(soundFile);
            return soundFile;
        }

        //.music-Dateien wo ich die darin enthaltenen Instrumente einzeln starten/stoppen will
        public IFrequenceToneSnipped[] AddSynthSoundCollection(string musicFile)
        {
            var multi = MultiSequenzer.LoadFromFile(musicFile, this.audioFileReader, this.SampleRate);
            multi.Volume = 1;
            var syntList = multi.GetAllSequenzers().Select(x => new FrequencyTone(x)).ToList();
            this.frequenzeToneList.AddRange(syntList);
            this.sampleProviders.AddRange(this.frequenzeToneList);
            return syntList.ToArray();
        }

        //.music-Dateien (Beispiel: Intro-Musik die aus mehreren Instrumenten besteht) -> Die Wiedergabe läßt alle Einzelsequenzer gleichzeitig abspielen
        public IAudioFileSnipped AddMusicFile(string musicFile)
        {
            var multi = MultiSequenzer.LoadFromFile(musicFile, this.audioFileReader, this.SampleRate);
            multi.Volume = 1;
            var soundFile = new MusicFile(multi);
            this.sampleProviders.Add(soundFile);
            return soundFile;
        }

        //.synt-Datei
        public IFrequenceToneSnipped AddFrequencyTone(string syntiFile)
        {
            var data = XmlHelper.LoadFromXmlFile<SynthesizerData>(syntiFile);
            PianoSequenzer sequenzer = this.frequenzTones.AddEmptySequenzer(new SequenzerSize(0, 127, 1));
            sequenzer.Synthesizer.SetAllSettings(data, this.audioFileReader, Path.GetDirectoryName(syntiFile), this.SampleRate);
            var tone = new FrequencyTone(sequenzer);
            this.frequenzeToneList.Add(tone);
            this.sampleProviders.Add(tone);
            return tone;
        }

        //Ton, der über IFrequenceToneSnipped.Synthesizer und IFrequenceToneSnipped.Frequency dann gesteuert wird 
        public IFrequenceToneSnipped AddFrequencyTone()
        {
            PianoSequenzer sequenzer = this.frequenzTones.AddEmptySequenzer(new SequenzerSize(0, 127, 1));
            var tone = new FrequencyTone(sequenzer);
            this.frequenzeToneList.Add(tone);
            return tone;
        }
    }
}
