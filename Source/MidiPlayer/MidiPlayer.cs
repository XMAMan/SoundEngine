using MidiParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiPlayer
{
    class MidiMessage
    {
        public int Message;
        public double TimeInMs;    //So viele Millisekunden nach File-Start tritt dieses Event auf
    }

    static class MidiFileParser
    {
        public static MidiMessage[] GetMessages(MidiFile midiFile)
        {
            //Schritt 1: Program-Messages für alle Kanäle ganz am Anfang
            List<MidiMessage> programMessages = new List<MidiMessage>();
            for (byte channel = 0; channel < midiFile.Instruments.Length; channel++)
            {
                int programMessage = CreateProgramChangeMessage((byte)midiFile.Instruments[channel].InstrumentName, channel);
                programMessages.Add(new MidiMessage() { Message = programMessage, TimeInMs = 0 });
            }

            //Schritt 2: NoteOn/NoteOff-Nachrichten 
            List<MidiMessage> noteMessages = new List<MidiMessage>();
            for (byte channel = 0; channel < midiFile.Instruments.Length; channel++)
            {
                foreach (var note in midiFile.Instruments[channel].Notes)
                {
                    if (note.Type == MidiNote.NoteType.On)
                    {
                        int noteOn = CreateNoteOnMessage(note.NoteNumber, (byte)(note.Volume * 127), channel);
                        noteMessages.Add(new MidiMessage() { Message = noteOn, TimeInMs = note.TimeInMs });
                    }

                    if (note.Type == MidiNote.NoteType.Off)
                    {
                        int noteOff = CreateNoteOffMessage(note.NoteNumber, channel);
                        noteMessages.Add(new MidiMessage() { Message = noteOff, TimeInMs = note.TimeInMs });
                    }
                }
            }
            programMessages.AddRange(noteMessages.OrderBy(x => x.TimeInMs));
            return programMessages.ToArray();
        }

        //http://blog.fourthwoods.com/2011/12/23/playing-midi-files-in-windows-part-1/
        //0x000003C0 -> 00-00 Nor Used | 03 = Instrument 3 | C = Program Change-Event | 0 = Channel 0
        private static int CreateProgramChangeMessage(byte instrument, byte channel)
        {
            return (instrument << 8) | 0xC0 | (channel & 0xF);
        }

        //0x00403C90 -> 00-Not Used | 40 Speed to Press | 3c 3. Note C | 9 = Press Down | 0 = Channel 0
        private static int CreateNoteOnMessage(byte noteIndex, byte velocity, byte channel)
        {
            return (velocity << 16) | (noteIndex << 8) | 0x90 | (channel & 0xF);
        }
        
        //0x00003C90 -> 00 Not Used |00 (Velocity; 0 =Key-Up) |3C Note 3C | 90 = Key Up (Eigentlich ist NoteOff 8 aber wenn man NoteOn mit Velocity=0 macht, entspricht das ein NoteOff
        private static int CreateNoteOffMessage(byte noteIndex, byte channel)
        {
            return  (noteIndex << 8) | 0x90 | (channel & 0xF);
        }
    }

    public class MidiPlayer
    {
        private MultimediaTimer timer;
        private MidiMessage[] messages;
        private int midiHandle = 0;
        private int messageIndex = 0;
        private DateTime startTime;

        public MidiPlayer(MidiFile midiFile)
        {
            this.messages = MidiFileParser.GetMessages(midiFile);
            this.timer = new MultimediaTimer();
            this.timer.Tick += TimerAction;
        }

        public void Start()
        {
            int err = MidiDllWraper.midiOutOpen(ref this.midiHandle, 0, null, 0, 0);
            this.timer.Start();
            this.startTime = DateTime.Now;
        }

        public void Stop()
        {
            this.timer.Stop();
            MidiDllWraper.midiOutClose(this.midiHandle);
        }

        private void TimerAction(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            double msToStart = (now - this.startTime).TotalMilliseconds;
            while (messageIndex < this.messages.Length && this.messages[this.messageIndex].TimeInMs < msToStart)
            {
                MidiDllWraper.midiOutShortMsg(this.midiHandle, this.messages[this.messageIndex].Message);
                this.messageIndex++;
            }
            if (messageIndex >= this.messages.Length)
            {
                Stop();
            }
        }
    }
}
