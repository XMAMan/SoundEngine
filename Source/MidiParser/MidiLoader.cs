using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MidiParser
{
    //Eine Midi-Datei besteht aus 1 bis 16 Kanälen. Jeder Kanal ist sozusagen ein Musiker, welcher nicht mehr als ein Instrument zeitgleich
    //in der Hand halten kann. Mit der Program-Nachricht sage ich für ein Kanal, welches Instrument dieser ab nun spielen soll.
    //Eine Midi-Datei ist vom Aufbau her so, dass erst der Header kommt, wo steht, wie viele Millisekunden ein Midi-Tick lang ist und
    //von welchen MidiTyp die Datei ist. Bei Typ 0 kommt nach dem Header nur ein Track. In diesen Track kommen die ganzen Midi-Event-Nachrichten,
    //welche alle als Angabe den Tick und den zugehörigen Kanal haben. Bei Typ 0 beschreibt man also 16 Musiker, die Zeitgleich alle jeweils
    //ein Instrument spielen(und dieses wärend des Konzerts auch wechseln könnten). Bei Midi-Typ-1-Dateien gibt es mehrere Tracks hintereinander
    //in der Datei. Die Kanal-Nummer von den Midi-Events entspricht der Track-Nummer. Jeden Track ist nur ein Kanal zugeordnet. Diese 16 Musiker
    //werden allso nicht zeitgleich sondern nacheinandner in der Datei alle jeweils einzeln beschrieben aber ihre Ticks beginnen auch alle bei 0.
    //D.h. sie spielen dann auch zeitgleich.

    class MidiLoader
    {
        //Wärend des Einladens nutze ich diese Hilfsklassen, damit jeden Kanal immer ein aktives Instrument zugeordnet ist, und allen Noten
        //somit der InstrumentenIndex zugeordnet werden kann. Am Ende kann ich dann die Noten nach Instrument statt nach Kanal gruppiert dann
        //ausgeben.
        class Note
        {
            public long TimeInTicks;
            public bool IsOn;
            public byte NoteNumber; //60 = Middle C
            public byte Velocity;
            public byte InstrumentIndex; //Dieses Instrument soll gespielt werden 
        }

        class Channel
        {
            public List<Note> Notes = new List<Note>();
            public byte UsedInstrument = 0; //Wenn kein Instrument bisher angegeben wurde, dann wird davon ausgegangen, dass Instrument 0 verwendet wird


            public void AddNote(long timeInTicks, bool isOne, byte noteNumber, byte velocity)
            {
                this.Notes.Add(new Note()
                {
                    TimeInTicks = timeInTicks,
                    IsOn = isOne,
                    NoteNumber = noteNumber,
                    Velocity = velocity,
                    InstrumentIndex = this.UsedInstrument
                });
            }
        }

        //Wärend des Parsens einer Midi-Datei ist das hier eine Hilfsklasse, welche die Noten, Program und TempoSetting-Nachrichten aufsammelt
        class MidiFileSimple
        {
            private Channel[] channels = new Channel[16];
            private ushort timeDivision; //Kommt aus dem Header
            private bool tempoSettingMessageWasReceived = false;
            private double microsecondsPerBeat = 500000; //Default-Value (Kommt aus einer MetaEvent-Nachricht vom Typ TempoSetting


            public MidiFileSimple(ushort timeDivision)
            {
                this.timeDivision = timeDivision;
                for (int i = 0; i < channels.Length; i++) this.channels[i] = new Channel();
            }

            public void AddNote(byte channel, long timeInTicks, bool isOne, byte noteNumber, byte velocity)
            {
                this.channels[channel].AddNote(timeInTicks, isOne, noteNumber, velocity);
            }

            public void AddSetProgram(byte channel, long timeInTicks, byte instrumentIndex)
            {
                this.channels[channel].UsedInstrument = instrumentIndex;
            }

            public void AddMetaEvent(long tick, byte metaType, byte[] data)
            {
                //Aufbau einer MetaEvent-TempoSetting-Nachricht: FF 51 03 tt tt tt    ->FF=MetaEvent;51=TempoSetting;03=MetaLengt=Anzahl der Bytes die noch kommen;  tt tt tt is a 24-bit value specifying the tempo as the number of microseconds per quarter note.
                if (metaType == 0x51 && tempoSettingMessageWasReceived == false) //Verarbeite nur die erste TempoSetting-Nachricht
                {
                    this.microsecondsPerBeat = (int)(data[0] << 16 | data[1] << 8 | data[2]);
                    this.tempoSettingMessageWasReceived = true;
                }
            }

            //http://www.recordingblogs.com/sa/Wiki/topic/Time-division-of-a-MIDI-file
            public double GetMillisecondsPerTick()
            {
                //ticks per beat" (or “pulses per quarter note”)
                if ((this.timeDivision & 0x8000) == 0)
                {
                    int midiTicksPerBeat = this.timeDivision & 0x7FFF;
                    double millisecondsPerTick = this.microsecondsPerBeat / midiTicksPerBeat / 1000; //1 Tick ist 'return' Millisekunen lang
                    return millisecondsPerTick;
                }
                else//frames per second
                {
                    double framesPerSecond = this.timeDivision & 0x7F00;
                    double ticksPerFrame = (this.timeDivision >> 8) & 0x7F;
                    return 1000 / (framesPerSecond * ticksPerFrame);
                }
            }

            private double ConvertMidiTimeToMilliseconds(double millisecondsPerTick, long midiTickTime)
            {
                return (midiTickTime * millisecondsPerTick);
            }

            public MidiFile GetMidiFile()
            {
                double millisecondsPerTick = GetMillisecondsPerTick();

                MidiInstrument[] instruments = this.channels.SelectMany(x => x.Notes).GroupBy(x => x.InstrumentIndex).OrderBy(x => x.Key).Select(x =>
                    new MidiInstrument()
                    {
                        InstrumentName = (GeneralMidiInstrument)x.Key,
                        Notes = x.OrderBy(y => y.TimeInTicks).Select(y => new MidiNote()
                        {
                            TimeInMs = ConvertMidiTimeToMilliseconds(millisecondsPerTick, y.TimeInTicks),
                            Type = (y.IsOn && y.Velocity > 0) ? MidiNote.NoteType.On : MidiNote.NoteType.Off,
                            NoteNumber = y.NoteNumber,
                            Volume = y.Velocity / 128f
                        }).ToArray()
                    }).ToArray();

                return MidiFile.FromData(instruments);
            }


        }

        class MidiHeader
        {
            public int MidiType;
            public int NumTracks = 1;
            public ushort TimeDivision;
        }

        //Jeder Kanal steht für ein Instrument. Die Program-Nachricht innerhalb des Kanals sagt, welches Instrument das ist.
        public static MidiFile LoadFile(string filePath)
        {
            VerifyMidi(filePath);   // this will raise an exception if the Midi file is invalid

            BinaryReader midiBinaryStream = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
            var header = ReadHeaderFromStream(midiBinaryStream);

            MidiFileSimple midiFile = new MidiFileSimple(header.TimeDivision);

            for (int i = 0; i < header.NumTracks; i++) ReadTrackFromStream(midiFile, midiBinaryStream);

            midiBinaryStream.BaseStream.Close();

            return midiFile.GetMidiFile();
        }

        private static void VerifyMidi(string filePath)     // throws an Exception if a BAD file was selected ; FINISHED
        {
            FileStream midiFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] midiHeaderString = new byte[4];
            midiFileStream.Position = 0;
            midiFileStream.Read(midiHeaderString, 0, 4);
            if (Encoding.ASCII.GetString(midiHeaderString, 0, 4) != "MThd")
                throw new Exception("MThd string wasn't found in the MIDI header!");
            if (midiFileStream.ReadByte() != 0x0 || midiFileStream.ReadByte() != 0x0 || midiFileStream.ReadByte() != 0x0 || midiFileStream.ReadByte() != 0x6)
                throw new Exception("MThd chunk size not #0x6!");
            midiFileStream.Position = 0xA;
            int numTracks = midiFileStream.ReadByte() << 8 | midiFileStream.ReadByte();
            if (numTracks == 0)
                throw new Exception("The MIDI has no tracks to convert!");
            midiFileStream.Close();
        }

        private static MidiHeader ReadHeaderFromStream(BinaryReader midiBinaryStream)
        {
            midiBinaryStream.BaseStream.Position = 9;    // position to the midi Type
            int midiType = midiBinaryStream.BaseStream.ReadByte();

            ushort timeDivision = 0;
            int numTracks = -1;

            switch (midiType)
            {
                case 0:
                    //LoadAndConvertTypeZero(filePath, midiTracks, ref timeDivision); //Converting and loading MIDI...
                    midiBinaryStream.BaseStream.Position = 0xC;      // seek to the amount of tracks in the MIDI file
                    timeDivision = (ushort)(midiBinaryStream.ReadByte() << 8 | midiBinaryStream.ReadByte());
                    numTracks = 1;
                    break;
                case 1:
                    //LoadDirectly(filePath, midiTracks, ref timeDivision); //Loading MIDI...
                    midiBinaryStream.BaseStream.Position = 0xA;      // seek to the amount of tracks in the MIDI file
                    numTracks = midiBinaryStream.ReadByte() << 8 | midiBinaryStream.ReadByte();
                    timeDivision = (ushort)(midiBinaryStream.ReadByte() << 8 | midiBinaryStream.ReadByte());
                    break;
                case 2:
                    throw new Exception("MIDI type 2 is not supported by this program!");
                default:
                    throw new Exception("Invalid MIDI type!");
            }

            return new MidiHeader() { MidiType = midiType, NumTracks = numTracks, TimeDivision = timeDivision };
        }

        enum NormalType
        {
            NoteON,
            NoteOFF,
            NoteAftertouch,
            Controller,
            Program,
            ChannelAftertouch,
            PitchBend
        }
        private static void ReadTrackFromStream(MidiFileSimple midiFile, BinaryReader midiBinaryStream)
        {
            long currentTick = 0;
            NormalType lastEventType = NormalType.NoteOFF;
            byte lastMidiChannel = 0;
            // check if the track doesn't begin like expected with an MTrk string
            byte[] textString = new byte[4];
            midiBinaryStream.Read(textString, 0, 4);
            if (Encoding.ASCII.GetString(textString, 0, 4) != "MTrk")
                throw new Exception("Track doesn't start with MTrk string!");
            byte[] intArray = new byte[4];
            midiBinaryStream.Read(intArray, 0, 4);    // read the track length
                                                      // this value isn't even needed, so we don't do further processing with it; I left it in the code for some usage in the future; no specific plan???

            // now do the event loop and load all the events
            #region EventLoop
            while (true)
            {
                // first thing that is done is getting the next delta length value and add the value to the current position to calculate the absolute position of the event
                currentTick += ReadVariableLengthValue(midiBinaryStream);//Delta-Time-Ticks

                // now check what event type is used and disassemble it

                byte eventTypeByte = midiBinaryStream.ReadByte();

                // do a jumptable for each event type

                if (eventTypeByte == 0xFF)      // if META Event
                {
                    byte metaType = midiBinaryStream.ReadByte();
                    long metaLength = ReadVariableLengthValue(midiBinaryStream);
                    byte[] metaData = new byte[metaLength];
                    midiBinaryStream.Read(metaData, 0, (int)metaLength);

                    if (metaType == 0x2F)
                        break;        // if end of track is reached, break out of the loop, End of Track Events aren't written into the objects

                    midiFile.AddMetaEvent(currentTick, metaType, metaData);
                }
                else if (eventTypeByte == 0xF0 || eventTypeByte == 0xF7)        // if SysEx Event
                {
                    long sysexLength = ReadVariableLengthValue(midiBinaryStream);
                    midiBinaryStream.Read(new byte[sysexLength], 0, (int)sysexLength);
                }
                else if (eventTypeByte >> 4 == 0x8)     // if Note OFF command
                {
                    lastEventType = NormalType.NoteOFF;
                    lastMidiChannel = (byte)(eventTypeByte & 0xF);
                    midiFile.AddNote(lastMidiChannel, currentTick, false, midiBinaryStream.ReadByte(), midiBinaryStream.ReadByte());
                }
                else if (eventTypeByte >> 4 == 0x9)     // if Note ON command
                {
                    lastEventType = NormalType.NoteON;
                    lastMidiChannel = (byte)(eventTypeByte & 0xF);
                    midiFile.AddNote(lastMidiChannel, currentTick, true, midiBinaryStream.ReadByte(), midiBinaryStream.ReadByte());
                }
                else if (eventTypeByte >> 4 == 0xC)     // if Preset command
                {
                    lastEventType = NormalType.Program;
                    lastMidiChannel = (byte)(eventTypeByte & 0xF);
                    midiFile.AddSetProgram(lastMidiChannel, currentTick, midiBinaryStream.ReadByte());
                }
                else if (eventTypeByte >> 4 == 0xA)     // if Aftertouch command
                {
                    byte par1 = midiBinaryStream.ReadByte();
                    byte par2 = midiBinaryStream.ReadByte();
                    lastEventType = NormalType.NoteAftertouch;
                    lastMidiChannel = (byte)(eventTypeByte & 0xF);
                }
                else if (eventTypeByte >> 4 == 0xB)     // if MIDI controller command
                {
                    byte par1 = midiBinaryStream.ReadByte();
                    byte par2 = midiBinaryStream.ReadByte();
                    lastEventType = NormalType.Controller;
                    lastMidiChannel = (byte)(eventTypeByte & 0xF);
                }
                else if (eventTypeByte >> 4 == 0xE)     // if Pitch Bend command
                {
                    byte par1 = midiBinaryStream.ReadByte();
                    byte par2 = midiBinaryStream.ReadByte();
                    lastEventType = NormalType.PitchBend;
                    lastMidiChannel = (byte)(eventTypeByte & 0xF);
                }
                else if (eventTypeByte >> 4 == 0xD)     // if Channel Aftertouch command
                {
                    byte par1 = midiBinaryStream.ReadByte();
                    //byte par2 = 0x0;    // unused
                    lastEventType = NormalType.ChannelAftertouch;
                    lastMidiChannel = (byte)(eventTypeByte & 0xF);
                }

                else if (eventTypeByte >> 4 < 0x8)
                {
                    byte par1 = eventTypeByte;
                    byte par2;
                    switch (lastEventType)
                    {
                        case NormalType.NoteOFF:
                            par2 = midiBinaryStream.ReadByte();
                            midiFile.AddNote(lastMidiChannel, currentTick, false, par1, par2);
                            break;
                        case NormalType.NoteON:
                            par2 = midiBinaryStream.ReadByte();
                            midiFile.AddNote(lastMidiChannel, currentTick, true, par1, par2);
                            break;
                        case NormalType.Program:
                            midiFile.AddSetProgram(lastMidiChannel, currentTick, par1);
                            break;
                        case NormalType.NoteAftertouch:
                        case NormalType.Controller:
                        case NormalType.PitchBend:
                            par2 = midiBinaryStream.ReadByte();
                            break;
                    }
                }
                else
                {
                    throw new Exception("Bad MIDI event at 0x" + midiBinaryStream.BaseStream.Position.ToString("X8") + ": 0x" + eventTypeByte.ToString("X2"));
                }
            }   // end of the event transscribing loop
            #endregion
        }

        private static long ReadVariableLengthValue(BinaryReader midiBinaryStream)     // reads a variable Length value from the Filestream at its current position and extends the Stream position by the exact amount of bytes ; FINSHED
        {
            long backupPosition = midiBinaryStream.BaseStream.Position;
            int numBytes = 0;
            while (true)
            {
                numBytes++;
                if ((midiBinaryStream.ReadByte() & 0x80) == 0)
                    break;
            }

            midiBinaryStream.BaseStream.Position = backupPosition;

            long returnValue = 0;

            for (int currentByte = 0; currentByte < numBytes; currentByte++)
            {
                returnValue = (returnValue << 7) | (byte)(midiBinaryStream.ReadByte() & 0x7F);
            }

            return returnValue;
        }
    }
}
