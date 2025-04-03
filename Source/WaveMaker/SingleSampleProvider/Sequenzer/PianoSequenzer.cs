using MidiParser;
using WaveMaker.KeyboardComponents;

namespace WaveMaker.Sequenzer
{
    public class SequenzerData
    {
        public GeneralMidiInstrument InstrumentName { get; set; }
        public float Volume { get; set; } = 0.5f;
        public bool IsEnabled { get; set; } = true;
        public float TestToneFrequence { get; set; } = 440;
        public SynthesizerData SynthesizerData { get; set; }

    }

    //Sowohl der leere Sequenzer besitzt eine bestimmte Breite/Höhe als auch das Rechteck, was seine Noten umschließt
    public class SequenzerSize
    {
        public int MinToneIndex { get; private set; }
        public int MaxToneIndex { get; private set; }
        public int MaxSamplePosition { get; private set; }

        public SequenzerSize(int minToneIndex, int maxToneIndex, int maxSamplePosition)
        {
            this.MinToneIndex = minToneIndex;
            this.MaxToneIndex = maxToneIndex;
            this.MaxSamplePosition = maxSamplePosition;
        }
    }

    //Ein einzelnes Piano, was genau einmal ein Lied abspielt
    public class PianoSequenzer
    {
        //Existiert nur so lange, wie eine bestimmte Taste gedrückt wird
        class RunningKey
        {
            public int KeyIndex { get; private set; } //Rückgabewert von piano.StartPlayingKey (Wird für piano.ReleaseKey benötigt)
            public int StopIndex { get; private set; } //Zu diesen Sampe-Zeitpunkt soll gestoppt werden

            public RunningKey(int keyIndex, int stopIndex)
            {
                this.KeyIndex = keyIndex;
                this.StopIndex = stopIndex;
            }
        }

        public Synthesizer Synthesizer { get { return this.piano.Synthesizer; } }
        public SequenzerKeys Notes { get; private set; } //Noten
        public GeneralMidiInstrument InstrumentName { get; private set; }
        public float Volume { get; set; } = 0.5f;//0.04f;
        public bool IsEnabled { get; set; } = true;
        public float TestToneFrequence { get; set; } = 440;
        public SequenzerSize MaxAllowedSize { get; private set; }
        public SequenzerSize CurrentNoteSize { get; private set; }
        public int KeyShift { get; set; } = 0; //Um so viele Oktaven wird beim Anspielen einer Taste alles noch verschoben

        private Piano piano;
        private IMultiSequenzer multiSequenzer; //Der Sequenzer muss dem MultiSequenzer sagen können, dass wenn sich Noten bei ihm geändert haben, die sequenzerWithLongestLength-Variable beim MultiSequenzer aktualisiert werden muss


        private List<RunningKey> allRunningTones = new List<RunningKey>();

        internal PianoSequenzer(MidiInstrument instrument, int sampleRate, IMultiSequenzer multiSequenzer, IAudioRecorder audioRecorder)
        {
            this.piano = new Piano(sampleRate, audioRecorder);
            this.Notes = new SequenzerKeys(instrument.Notes, piano.SampleRate);
            this.InstrumentName = instrument.InstrumentName;
            this.multiSequenzer = multiSequenzer;
            this.MaxAllowedSize = new SequenzerSize(this.Notes.MinToneIndex, this.Notes.MaxToneIndex, this.Notes.MaxSamplePosition);
            this.CurrentNoteSize = new SequenzerSize(this.Notes.MinToneIndex, this.Notes.MaxToneIndex, this.Notes.MaxSamplePosition);
        }

        internal PianoSequenzer(GeneralMidiInstrument instrumentName, SequenzerSize maxAllowedSize, int sampleRate, IMultiSequenzer multiSequenzer, IAudioRecorder audioRecorder)
        {
            this.piano = new Piano(sampleRate, audioRecorder);
            this.Notes = new SequenzerKeys(maxAllowedSize.MaxSamplePosition, piano.SampleRate);
            this.InstrumentName = instrumentName;
            this.multiSequenzer = multiSequenzer;
            this.MaxAllowedSize = maxAllowedSize;
            this.CurrentNoteSize = new SequenzerSize(this.Notes.MinToneIndex, this.Notes.MaxToneIndex, this.Notes.MaxSamplePosition);
        }

        public int SampleRate { get { return this.piano.SampleRate; } }


        public void SetSamplePosition(double samplePosition)
        {
            var nextKeysToStart = Notes.SetSampleIndex(samplePosition);
            foreach (var key in nextKeysToStart)
            {
                PlayTone(key, (int)this.sampleIndex);
            }
        }

        
        private double sampleIndex = 0; //ist vom Typ double und nicht int, um die Tastenanschlagsgeschwindigkeit ändern zu können
        public float GetNextSample(bool startNewKeys, float keyStrokeSpeed = 1)
        {
            //this.sampleIndex = Läuft von 0 bis Unendlich(Entspricht der Zeit). Wird hier benötigt, um die Stop-Key-Zeit zu hinterlegen. Das Stoppen geht auch dann, wenn IsPlaying false ist
            //this.Notes.SampleIndex = Läuft von 0 bis Midi-File-Ende (Entspricht den blauen Balken)
            //this.piano.keys.sampleData.SampleIndex = Läuft von 0 bis KeyUp-Index (Entspricht der Länge des Tastendrucks)
            this.sampleIndex += keyStrokeSpeed;

            //Spiele neue Töne an
            if (startNewKeys) //startNewKeys == IsPlaying
            {
                var nextKeysToStart = this.Notes.StartNextKeys(keyStrokeSpeed);
                foreach (var key in nextKeysToStart)
                {
                    PlayTone(key, (int)this.sampleIndex);
                }
            }

            //Beende alle alten Töne
            var removeTones = this.allRunningTones.Where(x => x.StopIndex < this.sampleIndex).ToList();
            foreach (var tone in removeTones)
            {
                this.piano.ReleaseKey(tone.KeyIndex);
                this.allRunningTones.Remove(tone);
            }

            //Lied ist durch?
            if (this.Notes.IsFinish)
            {
                ReleaseAllKeys();
            }

            if (this.IsEnabled == false) return 0;

            return this.piano.GetNextSample() * (this.Volume * this.Volume);
        }
      
        private void PlayTone(SequenzerKey key, int sampleIndex)
        {
            int keyIndex = this.piano.StartPlayingKey(key.NoteNumber + this.KeyShift * 12);
            if (keyIndex == Piano.NoMoreKeys) return;
            int stopIndex = sampleIndex + key.Length;

            this.allRunningTones.Add(new RunningKey(keyIndex, stopIndex));
        }

        public void ReleaseAllKeys()
        {
            this.piano.ReleaseAllKeys();
            this.allRunningTones.Clear();
        }

        public void PlayTone(SequenzerKey key)
        {
            PlayTone(key, (int)this.sampleIndex);
        }

        public void ResetPlayPosition()
        {
            this.piano.ReleaseAllKeys();
            this.allRunningTones.Clear();
            this.Notes.Reset();
        }

        public int StartPlayingKey(int toneIndex)
        {
            return this.piano.StartPlayingKey(toneIndex);
        }
        public int StartPlayingKey(float frequence)
        {
            return this.piano.StartPlayingKey(frequence);
        }

        public void ReleaseKey(int keyIndex)
        {
            this.piano.ReleaseKey(keyIndex);
        }

        public void SetToneIndexFromPlayingTone(int keyIndex, int toneIndex)
        {
            this.piano.SetToneIndexFromPlayingTone(keyIndex, toneIndex);
        }
        public void SetFrequencyFromPlayingTone(int keyIndex, float frequency)
        {
            this.piano.SetFrequencyFromPlayingTone(keyIndex, frequency);
        }

        public void UpdateNotes(SequenzerKey[] notes)
        {
            this.Notes.UpdateNotes(notes);
            this.CurrentNoteSize = new SequenzerSize(this.Notes.MinToneIndex, this.Notes.MaxToneIndex, this.Notes.MaxSamplePosition);
            this.multiSequenzer.UpdateAfterNoteChanges();           
        }

        public SequenzerData GetAllSettings()
        {
            return new SequenzerData()
            {
                InstrumentName = this.InstrumentName,
                Volume = this.Volume,
                IsEnabled = this.IsEnabled,
                TestToneFrequence = this.TestToneFrequence,
                SynthesizerData = this.Synthesizer.GetAllSettings()
            };
        }

        public void SetAllSettings(SequenzerData data, IAudioFileReader audioFileReader, string searchDirectoryForAudioFiles)
        {
            this.Synthesizer.SetAllSettings(data.SynthesizerData, audioFileReader, searchDirectoryForAudioFiles, this.SampleRate);
            this.Volume = data.Volume;
            this.IsEnabled = data.IsEnabled;
            this.TestToneFrequence = data.TestToneFrequence;
        }
    }
}
