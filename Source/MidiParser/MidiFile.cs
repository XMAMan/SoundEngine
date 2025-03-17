using System.Linq;

namespace MidiParser
{
    public class MidiNote
    {
        public enum NoteType { On, Off }
        public NoteType Type;
        public double TimeInMs;    //So viele Millisekunden nach File-Start tritt dieses Event auf
        public byte NoteNumber; //60 = Middle C
        public float Volume;    //Lautstärke 0..1
    }

    public class MidiInstrument
    {
        public MidiNote[] Notes;
        public GeneralMidiInstrument InstrumentName; //Dieses Instrument soll gespielt werden
    }

    public class MidiFile
    {
        public MidiInstrument[] Instruments { get; private set; }

        public static MidiFile FromFile(string filePath)
        {
            return MidiLoader.LoadFile(filePath);
        }

        public void WriteToFile(string filePath)
        {
            MidiSaver.SaveToFile(filePath, this);
        }

        public static MidiFile FromData(MidiInstrument[] instruments)
        {
            return new MidiFile() { Instruments = instruments };
        }

        public string TestAusgabe()
        {
            return string.Join(System.Environment.NewLine, this.Instruments.Select(x => "Instrument=" + x.InstrumentName + System.Environment.NewLine + string.Join(System.Environment.NewLine, x.Notes.Select(y => y.TimeInMs.ToString().Replace(",", ".") + "\t" + y.Type + " " + y.NoteNumber))));
        }
    }
}
