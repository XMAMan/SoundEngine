﻿using MidiParser;
using WaveMaker.Helper;

namespace WaveMaker.Sequenzer
{
    public class MultiSequenzerData
    {
        public string MidiFileFileName { get; set; }
        public float LengthInMillisecondsForNewCreatedNotes { get; set; } = 250;
        public bool SnapToGrid { get; set; } = true;
        public int SampleCount { get; set; }
        public int MinToneIndex { get; set; }
        public int MaxToneIndex { get; set; }
        public List<SequenzerData> SynthesizerData { get; set; }
        public float KeyStrokeSpeed { get; set; } = 1;
    }

    interface IMultiSequenzer
    {
        void UpdateAfterNoteChanges();
    }

    //Verbund von mehreren Sequenzern die alle gleichzeitig abgespielt werden
    public class MultiSequenzer : ISingleSampleProvider, IMultiSequenzer
    {
        private List<PianoSequenzer> Sequenzers = new List<PianoSequenzer>();
        private PianoSequenzer sequenzerWithLongestLength = null;
        private IAudioRecorder audioRecorder;

        public static MultiSequenzer LoadFromFile(string musicFile, IAudioFileReader audioFileReader, int sampleRate, IAudioRecorder audioRecorder)
        {
            MultiSequenzer multi = new MultiSequenzer(sampleRate, audioRecorder);
            var data = XmlHelper.LoadFromXmlFile<MultiSequenzerData>(musicFile);
            multi.SetAllSettings(data, Path.GetDirectoryName(musicFile), audioFileReader);
            return multi;
        }

        public IEnumerable<PianoSequenzer> GetAllSequenzers()
        {
            return Sequenzers.ToList();
        }

        public MultiSequenzer(int sampleRate, IAudioRecorder audioRecorder)
        {
            this.SampleRate = sampleRate;
            this.audioRecorder = audioRecorder;
        }

        public MultiSequenzerData GetAllSettings()
        {
            return new MultiSequenzerData()
            {
                SampleCount = this.MaxAllowedSize.MaxSamplePosition,
                MinToneIndex = this.MaxAllowedSize.MinToneIndex,
                MaxToneIndex = this.MaxAllowedSize.MaxToneIndex,
                KeyStrokeSpeed = this.KeyStrokeSpeed,
                SynthesizerData = this.GetAllSynthesizerData()
            };
        }

        //Wenn der NAduio-Timer den MultiSequenzer als Sample-Provider festhält, dann kann ich dem MultiSequenzerViewModel nicht einfach ein neues Model zuweisen sondern muss das vorhandene Model beim Laden modifzieren
        //searchFolder = Hier liegen die Midi- und AudioFile-Dateien
        public void SetAllSettings(MultiSequenzerData data, string searchFolder, IAudioFileReader audioFileReader)
        {
            this.Sequenzers.Clear();
            this.sequenzerWithLongestLength = null;

            this.MaxAllowedSize = new SequenzerSize(data.MinToneIndex, data.MaxToneIndex, data.SampleCount);

            string midiFileName = searchFolder + "\\" + data.MidiFileFileName;
            var midiFile = File.Exists(midiFileName) ? MidiFile.FromFile(midiFileName) : null;
            if (midiFile != null && midiFile.Instruments.Any())
            {
                this.AddMidiFile(midiFile);
                foreach (var sequenzer in this.Sequenzers)
                {
                    var sequenzerData = data.SynthesizerData.First(x => x.InstrumentName == sequenzer.InstrumentName);
                    sequenzer.SetAllSettings(sequenzerData, audioFileReader, searchFolder);
                }
            }
            else
            {
                foreach (var emptySequenzerData in data.SynthesizerData)
                {
                    var emptySequenzer = this.AddEmptySequenzer(new SequenzerSize(2 * 12, 6 * 12, 25 * this.SampleRate));
                    emptySequenzer.SetAllSettings(emptySequenzerData, audioFileReader, searchFolder);
                }
            }

            this.KeyStrokeSpeed = data.KeyStrokeSpeed;
        }

        public void ClearAllSequenzers()
        {
            this.Sequenzers.Clear();
            this.sequenzerWithLongestLength = null;
            this.IsRunning = false;
            this.AutoLoop = false;
            this.KeyStrokeSpeed = 1;            
        }

        public SequenzerSize MaxAllowedSize { get; set; }
        public SequenzerSize CurrentNoteSize { get; private set; }

        public int SampleRate { get; private set; } 

        public bool IsRunning { get; set; }
        public float Volume { get; set; } = 0.01f;

        public int SampleCount { get { return this.sequenzerWithLongestLength != null ? this.sequenzerWithLongestLength.Notes.MaxSamplePosition : 0; } }
        public double CurrentPosition //Geht von 0 bis SampleCount
        { 
            get 
            { 
                return this.sequenzerWithLongestLength != null ? this.sequenzerWithLongestLength.Notes.SampleIndex : 0; 
            }
            set
            {
                foreach (var sequenzer in this.Sequenzers)
                {
                    sequenzer.SetSamplePosition(value);                    
                }
            }
        } 
        public bool AutoLoop { get; set; } = false;
        public bool IsFinish { get { return this.sequenzerWithLongestLength != null ? this.sequenzerWithLongestLength.Notes.IsFinish : false; } } //Ist das Lied durchgespielt?

        public float KeyStrokeSpeed { get; set; } = 1; //1 = Spiele in normaler Geschwindigkeit, 2 = Doppelt so schnell, 0.5 = Halbe Geschwindigkeit

        public PianoSequenzer[] AddMidiFile(MidiFile midiFile)
        {
            var newItems = midiFile.Instruments.Select(instrument => new PianoSequenzer(instrument, this.SampleRate, this, this.audioRecorder)).ToArray();
            Sequenzers.AddRange(newItems);

            UpdateMaxAllowedSize();
            UpdateAfterNoteChanges();

            return newItems;
        }

        public PianoSequenzer AddEmptySequenzer(SequenzerSize maxAllowedSize)
        {
            var newItem = new PianoSequenzer(GetNextFreeInstrument(), maxAllowedSize, this.SampleRate, this, this.audioRecorder);
            Sequenzers.Add(newItem);

            UpdateMaxAllowedSize();
            UpdateAfterNoteChanges();

            return newItem;
        }

        public void RemoveSequenzer(PianoSequenzer sequenzer)
        {
            this.Sequenzers.Remove(sequenzer);
            UpdateMaxAllowedSize();
        }

        private void UpdateMaxAllowedSize()
        {
            int minToneIndex = this.Sequenzers.Select(x => x.MaxAllowedSize.MinToneIndex).DefaultIfEmpty().Min();
            int maxToneIndex = this.Sequenzers.Select(x => x.MaxAllowedSize.MaxToneIndex).DefaultIfEmpty().Max();
            int maxSamplePosition = this.Sequenzers.Select(x => x.MaxAllowedSize.MaxSamplePosition).DefaultIfEmpty().Max();
            this.MaxAllowedSize = new SequenzerSize(minToneIndex, maxToneIndex, maxSamplePosition);
        }

        private void UpdateCurrentNoteSize()
        {
            int minToneIndex = this.Sequenzers.Select(x => x.CurrentNoteSize.MinToneIndex).DefaultIfEmpty().Min();
            int maxToneIndex = this.Sequenzers.Select(x => x.CurrentNoteSize.MaxToneIndex).DefaultIfEmpty().Max();
            int maxSamplePosition = this.Sequenzers.Select(x => x.CurrentNoteSize.MaxSamplePosition).DefaultIfEmpty().Max();
            this.CurrentNoteSize = new SequenzerSize(minToneIndex, maxToneIndex, maxSamplePosition);
        }

        public void UpdateAfterNoteChanges()
        {
            this.sequenzerWithLongestLength = this.Sequenzers.First(y => y.Notes.MaxSamplePosition == this.Sequenzers.Max(x => x.Notes.MaxSamplePosition));
            UpdateCurrentNoteSize();
        }

        private GeneralMidiInstrument GetNextFreeInstrument()
        {
            for (int i = 0; i < 127; i++)
                if (this.Sequenzers.Any(x => x.InstrumentName == (GeneralMidiInstrument)i) == false) return (GeneralMidiInstrument)i;

            throw new Exception("No free Instrument found");
        }

        public MidiFile GetNotesAsMidiFile()
        {
            return MidiFile.FromData(this.Sequenzers.Select(x =>
                new MidiInstrument()
                {
                    InstrumentName = x.InstrumentName,
                    Notes = x.Notes.GetAsMidiNotes(this.SampleRate)
                })
                .ToArray());
        }

        public bool ContainsAnyNotes()
        {
            return this.Sequenzers.Any(x => x.Notes.Notes.Any());
        }

        public List<SequenzerData> GetAllSynthesizerData()
        {
            return this.Sequenzers.Select(x => x.GetAllSettings()).ToList();
        }

        public float GetNextSample()
        {
            //if (this.IsRunning == false) return 0; //Wenn ich das so mache, können die Töne nicht durch verschieben der PlayPosition mit der Maus angespielt werden

            if (this.IsFinish && this.AutoLoop)
            {
                foreach (var sequenzer in this.Sequenzers.ToList())
                {
                    sequenzer.ResetPlayPosition();
                }
            }

            float sum = 0;
            foreach (var sequenzer in this.Sequenzers.ToList())
            {
                sum += sequenzer.GetNextSample(this.IsRunning, this.KeyStrokeSpeed);
            }

            //8-Bit-Effekt (Das Veringern der Bitrate oder Samplingrate nennt man Bit-Crushing und ist ein möglicher Verzerrungseffekt) -> Siehe: https://blog.landr.com/de/audio-effekte-eine-anleitung-die-dir-dabei-hilft-deinen-sound-zu-formen/#distortion
            //int stepFactor = 128;
            //sum = (float)(Math.Floor(sum * stepFactor) / stepFactor); //Treppen-Effekt

            return sum * this.Volume;
        }

        public float[] GetAllSamples()
        {
            float[] data = new float[this.CurrentNoteSize.MaxSamplePosition];
            foreach (var sequenzer in this.Sequenzers.ToList())
            {
                sequenzer.ResetPlayPosition();
            }

            for (int sampleIndex = 0;sampleIndex < data.Length; sampleIndex++)
            {
                float sample = 0;
                foreach (var sequenzer in this.Sequenzers.ToList())
                {
                    sample += sequenzer.GetNextSample(true);
                }
                sample *= this.Volume;
                data[sampleIndex] = sample;
            }
            return data;
        }
    }
}
