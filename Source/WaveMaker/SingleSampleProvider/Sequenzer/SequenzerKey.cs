using MidiParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveMaker.Sequenzer
{
    //Menge aller Noten, die ein Sequenzer spielen soll. Entspricht einer Midi-Datei nur dass die On und Off-Signale für eine Taste zu ein Block(SequenzerKey) zusammengefasst wurden
    public class SequenzerKeys
    {
        public SequenzerKey[] Notes { get; private set; }
        public int SampleIndex { get; private set; } = 0; //Aktuelle Spielposition. Geht von 0 bis MaxIndex
        public int MinToneIndex { get; private set; } = 0; //Aktuell kleinster ToneIndex
        public int MaxToneIndex { get; private set; } = 0; //Aktuell größter ToneIndex
        public int MaxSamplePosition { get; private set; } = 0; //Aktuell größte Sampleposition

        private int currentToneIndex;
        private int nextStartIndex;     //Zeigt auf den nächsten Index, bei dem ein neue Ton beginnt
        private bool noMoreKeysToStart = false;
        public bool IsFinish { get; private set; } = false; //Befindet sich der SampleIndex auf der MaxIndex-Position?
        public SequenzerKeys(MidiNote[] notes, int sampleRate)
        {
            var keyNotes = CreateFromMidiNotes(notes, sampleRate);

            UpdateNotes(keyNotes);
        }

        //Initial leerer Sequenzer
        public SequenzerKeys(int maxSampleIndex, int sampleRate)
        {
            UpdateNotes(new SequenzerKey[0]);
            this.MaxSamplePosition = maxSampleIndex;
        }

        public void OrderNotes()
        {
            this.Notes = this.Notes.OrderBy(x => x.StartByteIndex).ToArray();
        }

        public void UpdateNotes(SequenzerKey[] notes)
        {
            this.Notes = notes;
            this.MinToneIndex = this.Notes.Select(x => x.NoteNumber).DefaultIfEmpty().Min();
            this.MaxToneIndex = this.Notes.Select(x => x.NoteNumber).DefaultIfEmpty().Max() + 2;
            this.MaxSamplePosition = this.Notes.Select(x => x.StartByteIndex + x.Length).DefaultIfEmpty().Max();
            int pos = this.SampleIndex;
            Reset();
            SetSampleIndex(pos);
        }

        private static SequenzerKey[] CreateFromMidiNotes(MidiNote[] notes, int sampleRate)
        {
            List<SequenzerKey> result = new List<SequenzerKey>();

            List<MidiNote> pressedKeys = new List<MidiNote>();

            for (int i = 0; i < notes.Length; i++)
            {
                MidiNote note = notes[i];
                if (note.Type == MidiNote.NoteType.On)
                {
                    pressedKeys.Add(note);
                }
                if (note.Type == MidiNote.NoteType.Off && pressedKeys.Any(x => x.NoteNumber == note.NoteNumber))
                {
                    var onNote = pressedKeys.First(x => x.NoteNumber == note.NoteNumber);
                    int startIndex = (int)(sampleRate * onNote.TimeInMs / 1000);
                    int stopIndex = (int)(sampleRate * note.TimeInMs / 1000);

                    SequenzerKey key = new SequenzerKey()
                    {
                        StartByteIndex = startIndex,
                        Length = Math.Max(1, stopIndex - startIndex + 1),
                        Volume = onNote.Volume,
                        NoteNumber = note.NoteNumber
                    };

                    result.Add(key);
                    pressedKeys.Remove(onNote);
                }
            }

            return result.OrderBy(x => x.StartByteIndex).ToArray();
        }

        public MidiNote[] GetAsMidiNotes(int sampleRate)
        {
            List<MidiNote> notes = new List<MidiNote>();
            foreach (var key in this.Notes)
            {
                notes.Add(new MidiNote()
                {
                    Type = MidiNote.NoteType.On,
                    TimeInMs = key.StartByteIndex / (double)sampleRate * 1000,
                    NoteNumber = key.NoteNumber,
                    Volume = key.Volume
                });

                notes.Add(new MidiNote()
                {
                    Type = MidiNote.NoteType.Off,
                    TimeInMs = (key.StartByteIndex + key.Length - 1) / (double)sampleRate * 1000,
                    NoteNumber = key.NoteNumber,
                    Volume = key.Volume
                });
            }

            return notes.OrderBy(x => x.TimeInMs).ToArray();
        }

        public void Reset()
        {
            this.SampleIndex = 0;
            this.currentToneIndex = 0;
            this.nextStartIndex = this.Notes.Length > 0 ? this.Notes[0].StartByteIndex : int.MaxValue;
            this.noMoreKeysToStart = false;
            this.IsFinish = false;
        }

        public SequenzerKey[] StartNextKeys()
        {
            if (this.IsFinish) return new SequenzerKey[0];
            this.SampleIndex++;
            //this.SampleIndex += 10;
            if (this.SampleIndex >= this.MaxSamplePosition) this.IsFinish = true;
            if (this.noMoreKeysToStart) return new SequenzerKey[0];

            List<SequenzerKey> nextKeys = new List<SequenzerKey>();
            while (this.SampleIndex >= this.nextStartIndex)
            {
                nextKeys.Add(this.Notes[this.currentToneIndex++]);
                if (this.currentToneIndex >= this.Notes.Length)
                {
                    this.noMoreKeysToStart = true;
                    return nextKeys.ToArray();
                }
                this.nextStartIndex = this.Notes[this.currentToneIndex].StartByteIndex;
            }
            
            return nextKeys.ToArray();
        }

        //maxSampleIndex = Kommt vom MultiSequenzer. Ist der größe MaxIndex-Wert von allen Sequenzern 
        public SequenzerKey[] SetSampleIndex(int sampleIndex)
        {
            if (this.Notes.Length == 0) return new SequenzerKey[0];

            if (sampleIndex > this.MaxSamplePosition)
            {
                this.SampleIndex = this.MaxSamplePosition;
                this.IsFinish = true;
                return new SequenzerKey[0];
            }

            if (sampleIndex <= this.Notes[0].StartByteIndex)
            {
                this.SampleIndex = sampleIndex;
                this.currentToneIndex = 0;
                this.nextStartIndex = this.Notes[0].StartByteIndex;
                this.noMoreKeysToStart = false;
                this.IsFinish = false;
                return new SequenzerKey[0];
            }

            int oldToneIndex = this.currentToneIndex;

            this.SampleIndex = sampleIndex;
            this.currentToneIndex = SearchToneIndex(sampleIndex);
            this.noMoreKeysToStart = this.currentToneIndex >= this.Notes.Length;
            this.IsFinish = this.SampleIndex >= this.MaxSamplePosition;
            this.nextStartIndex = this.noMoreKeysToStart == false ? this.Notes[this.currentToneIndex].StartByteIndex : -1;            

            //Spiele alle Töne an, die dazwischen liegen
            if (this.currentToneIndex > oldToneIndex)
            {
                List<SequenzerKey> nextKeys = new List<SequenzerKey>();
                for (int i = oldToneIndex; i < this.currentToneIndex; i++)
                    if (sampleIndex >= this.Notes[i].StartByteIndex)
                    {
                        nextKeys.Add(this.Notes[i]);
                    }
                return nextKeys.ToArray();
            }

            return new SequenzerKey[0];
        }

        //Suche den Index für den gilt: this.Notes[index - 1].StartByteIndex < sampleIndex && this.Notes[index].StartByteIndex >= sampleIndex
        private int SearchToneIndex(int sampleIndex)
        {
            for (int index = 1; index < this.Notes.Length;index++)
            {
                if (this.Notes[index - 1].StartByteIndex < sampleIndex && this.Notes[index].StartByteIndex >= sampleIndex) return index;
            }
            return this.Notes.Length - 1;
        }
    }

    //Key-Down und Key-Up-Signal aus Midi-Datei legt Start/Ende-Zeit (Angsbe in Samples) für ein einzelnes Instrument fest
    [Serializable] //Ist Serialisierbar, damit es in die Zwischenablage kopiert werden kann
    public class SequenzerKey
    {
        public int StartByteIndex;
        public int Length; //Anzahl der Samples
        public float Volume; //Lautstärke (0..1)
        public byte NoteNumber; //60 = Middle C
    }
}
