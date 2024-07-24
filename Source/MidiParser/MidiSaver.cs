using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiParser
{
    class MidiSaver
    {
        public static void SaveToFile(string filePath, MidiFile easyMidi)
        {
            Console.WriteLine("Saving MIDI to type 1 file...");
            // first of all check if a file with the name already exists
            if (File.Exists(filePath)) File.Delete(filePath);
            FileStream midiFileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None);
            BinaryWriter midiWriter = new BinaryWriter(midiFileStream);

            //double microsecondsPerBeat = 500000; //Default-Value
            double microsecondsPerBeat = 499999;
            ushort timeDivision = 0x400; //60 Ticks per Beat
            //timeDivision |= 0x8000; //Setze das höchte Bit auf 1, um den 'ticks per beat" (or “pulses per quarter note”)'-Modus zu verwenden
            int midiTicksPerBeat = timeDivision & 0x7FFF;
            double millisecondsPerTick = microsecondsPerBeat / midiTicksPerBeat / 1000;


            // first of all write MIDI header string
            midiWriter.Write(Encoding.ASCII.GetBytes("MThd"));
            // writer the header chunk length (=6)
            midiWriter.Write(intToBigEndian(6));
            // write the midi file type (=1)
            midiWriter.Write(ushortToBigEndian(1));
            // write the amount of tracks
            midiWriter.Write(ushortToBigEndian((ushort)easyMidi.Instruments.Length));
            // write the time division
            midiWriter.Write(ushortToBigEndian(timeDivision));
            // finished writing the header, now do the tracks

            for (int currentTrack = 0; currentTrack < easyMidi.Instruments.Length; currentTrack++)
            {
                // write the header info
                midiWriter.Write(Encoding.ASCII.GetBytes("MTrk"));
                midiWriter.Write((int)0);           //4 Byte Platzhalter. Hier kommt dann die Track-Länge in Bytes rein

                long trackStartPosition = midiWriter.BaseStream.Position;

                //Schreibe microsecondsPerBeat (Tempo)
                midiWriter.Write(ConvertToVariableLength(0));
                midiWriter.Write((byte)0xff); //MetaEvent
                midiWriter.Write((byte)0x51); //Tempo
                midiWriter.Write(ConvertToVariableLength(3));
                midiWriter.Write(intTo3Byte((int)microsecondsPerBeat));

                //Time-Signature
                midiWriter.Write(ConvertToVariableLength(0));
                midiWriter.Write((byte)0xff); //MetaEvent
                midiWriter.Write((byte)0x58); //TimeSignature
                midiWriter.Write(ConvertToVariableLength(4));
                midiWriter.Write((byte)0x04); //numerator of the time signature
                midiWriter.Write((byte)0x02); //denominator of the time signature as a negative power of 2
                midiWriter.Write((byte)0x18); //number of MIDI clocks between metronome clicks
                midiWriter.Write((byte)0x08); //number of notated 32nd-notes in a MIDI quarter-note (24 MIDI Clocks)

                //Schreibe zuerst die Program-Nachricht, um das Instrument festzulegen
                midiWriter.Write(ConvertToVariableLength(0));
                midiWriter.Write((byte)0xC0);
                midiWriter.Write((byte)easyMidi.Instruments[currentTrack].InstrumentName);

                //foreach (var note in easyMidi.Instruments[currentTrack].Notes)
                for (int j = 0; j < easyMidi.Instruments[currentTrack].Notes.Length; j++)
                {
                    var note = easyMidi.Instruments[currentTrack].Notes[j];
                    double timeDiffInMs = easyMidi.Instruments[currentTrack].Notes[j].TimeInMs - (j > 0 ? easyMidi.Instruments[currentTrack].Notes[j - 1].TimeInMs : 0);
                    long tickDiff = (long)(Math.Round(timeDiffInMs / millisecondsPerTick));
                    midiWriter.Write(ConvertToVariableLength(tickDiff));

                    if (note.Type == MidiNote.NoteType.On)
                    {
                        midiWriter.Write((byte)0x90);
                        midiWriter.Write(note.NoteNumber);
                        midiWriter.Write((byte)(note.Volume * 127));
                    }
                    if (note.Type == MidiNote.NoteType.Off)
                    {
                        midiWriter.Write((byte)0x80);
                        midiWriter.Write(note.NoteNumber);
                        midiWriter.Write((byte)(note.Volume * 127));
                    }
                }

                midiWriter.Write((byte)0x0);    // write delta time for track end
                midiWriter.Write((byte)0xFF);   // write META event byte
                midiWriter.Write((byte)0x2F);   // write End of Track byte
                midiWriter.Write((byte)0x0);    // the length of this META event data is 0 (no data follows)

                long trackEndPosition = midiWriter.BaseStream.Position;   // calc the length of the event data and backup the position
                midiWriter.BaseStream.Position = trackStartPosition - 4;
                midiWriter.Write(intToBigEndian((int)(trackEndPosition - trackStartPosition)));
                midiWriter.BaseStream.Position = trackEndPosition;
            }

            midiWriter.BaseStream.Close();
        }

        private static byte[] ushortToBigEndian(ushort value)   // returns a byte Array in Big Endian of an ushort ; FINISHED
        {
            byte[] dataValues = new byte[2];
            dataValues[0] = (byte)(value >> 8);
            dataValues[1] = (byte)(value & 0xFF);
            return dataValues;
        }

        private static byte[] intToBigEndian(int value)         // returns a byte Array in Big Endian of an int ; FINISHED
        {
            byte[] dataValues = new byte[4];
            dataValues[0] = (byte)(value >> 24);
            dataValues[1] = (byte)((value >> 16) & 0xFF);
            dataValues[2] = (byte)((value >> 8) & 0xFF);
            dataValues[3] = (byte)(value & 0xFF);
            return dataValues;
        }

        private static byte[] intTo3Byte(int value)
        {
            byte[] dataValues = new byte[3];
            dataValues[0] = (byte)((value >> 16) & 0xFF);
            dataValues[1] = (byte)((value >> 8) & 0xFF);
            dataValues[2] = (byte)(value & 0xFF);
            return dataValues;
        }

        private static byte[] ConvertToVariableLength(long value)
        {
            int i = 0;
            byte[] returnData = new byte[i + 1];
            returnData[i] = (byte)(value & 0x7F);
            i++;

            value = value >> 7;

            while (value != 0)
            {
                Array.Resize(ref returnData, i + 1);
                returnData[i] = (byte)((value & 0x7F) | 0x80);
                value = value >> 7;
                i++;
            }

            Array.Reverse(returnData);
            return returnData;
        }
    }
}
