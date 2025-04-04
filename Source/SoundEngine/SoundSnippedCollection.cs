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
        private IAudioRecorder audioRecorder;

        public SoundSnippedCollection(IAudioFileReader audioFileReader, int sampleRate, IAudioRecorder audioRecorder, ISingleSampleProvider audioRecorderSnipped)
        {
            this.SampleRate = sampleRate;
            this.audioFileReader = audioFileReader;
            this.audioRecorder = audioRecorder;

            this.frequenzTones = new MultiSequenzer(this.SampleRate, this.audioRecorder);

            this.frequenzTones.Volume = 1;

            this.sampleProviders.Add(audioRecorderSnipped);
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
            AddSoundSnipped(soundFile);
            return soundFile;
        }

        //.music-Dateien wo ich die darin enthaltenen Instrumente einzeln starten/stoppen will
        public IFrequenceToneSnipped[] AddSynthSoundCollection(string musicFile)
        {
            var multi = MultiSequenzer.LoadFromFile(musicFile, this.audioFileReader, this.SampleRate, this.audioRecorder);
            multi.Volume = 1;
            var syntList = multi.GetAllSequenzers().Select(x => new FrequencyTone(x)).ToList();
            foreach (var synt in syntList)
            {
                this.AddSoundSnipped(synt);
            }
            return syntList.ToArray();
        }

        //.music-Dateien (Beispiel: Intro-Musik die aus mehreren Instrumenten besteht) -> Die Wiedergabe läßt alle Einzelsequenzer gleichzeitig abspielen
        public IMusicFileSnipped AddMusicFile(string musicFile)
        {
            var multi = MultiSequenzer.LoadFromFile(musicFile, this.audioFileReader, this.SampleRate, this.audioRecorder);
            multi.Volume = 1;
            var soundFile = new MusicFile(multi);
            AddSoundSnipped(soundFile);
            return soundFile;
        }

        //.synt-Datei
        public IFrequenceToneSnipped AddFrequencyTone(string syntiFile)
        {
            var data = XmlHelper.LoadFromXmlFile<SynthesizerData>(syntiFile);
            PianoSequenzer sequenzer = this.frequenzTones.AddEmptySequenzer(new SequenzerSize(0, 127, 1));
            sequenzer.Synthesizer.SetAllSettings(data, this.audioFileReader, Path.GetDirectoryName(syntiFile), this.SampleRate);
            var tone = new FrequencyTone(sequenzer);
            AddSoundSnipped(tone);
            return tone;
        }

        //Ton, der über IFrequenceToneSnipped.Synthesizer und IFrequenceToneSnipped.Frequency dann gesteuert wird 
        public IFrequenceToneSnipped AddFrequencyTone()
        {
            PianoSequenzer sequenzer = this.frequenzTones.AddEmptySequenzer(new SequenzerSize(0, 127, 1));
            var tone = new FrequencyTone(sequenzer);
            return tone;
        }

        private void AddSoundSnipped(ISoundSnipped snipped)
        {
            this.sampleProviders.Add(snipped);
            snipped.CopyWasCreated += this.AddSoundSnipped;
            snipped.DisposeWasCalled += this.RemoveSoundSnipped;
        }

        private void RemoveSoundSnipped(ISoundSnipped snipped)
        {
            this.sampleProviders.Remove(snipped);
        }
    }
}
