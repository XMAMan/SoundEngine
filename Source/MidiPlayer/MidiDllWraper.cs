using System.Runtime.InteropServices;
using System.Text;

namespace MidiPlayer
{
    //Wrapper für die 32-Bit winmm.dll, welche nur verwendet werden kann, wenn das Projekt als Zielplattform auf x86 gestellt ist
    public class MidiDllWraper
    {
        [DllImport("winmm.dll")]
        public static extern int midiOutReset(int handle); //Schaltet auf allen Kanälen die Musik ab

        [DllImport("winmm.dll")]
        public static extern int midiOutShortMsg(int handle, int message);//Sendet eine Nachricht mit 2 Parametern (8,9,A,B,C,D,E)

        [DllImport("winmm.dll")]
        public static extern int midiOutPrepareHeader(int handle,//Bereitet das MIDI-Gerät darauf vor, eine SysExMessage mit vielen Bytes zu empfangen
            IntPtr headerPtr, int sizeOfMidiHeader);

        [DllImport("winmm.dll")]
        public static extern int midiOutUnprepareHeader(int handle,
            IntPtr headerPtr, int sizeOfMidiHeader);

        [DllImport("winmm.dll")]
        public static extern int midiOutLongMsg(int handle,//Sendet eine SysExMessage
            IntPtr headerPtr, int sizeOfMidiHeader);

        [DllImport("winmm.dll")]
        public static extern int midiOutGetDevCaps(int deviceID,//Abfrage der Hardwaredaten vom Midi-Gerät (Und speichern in MidiOutCaps)
            ref MidiOutCaps caps, int sizeOfMidiOutCaps);

        [DllImport("winmm.dll")]
        public static extern int midiOutGetNumDevs();//Abfrage der Midi-Ausgabegeräteanzahl

        [DllImport("winmm.dll")]
        public static extern int midiConnect(int handleA, int handleB, int reserved);//Connects a MIDI InputDevice to a MIDI thru or OutputDevice, or connects a MIDI thru device to a MIDI OutputDevice. 

        [DllImport("winmm.dll")]
        public static extern int midiDisconnect(int handleA, int handleB, int reserved);


        [DllImport("winmm.dll")]
        public static extern int midiOutOpen(ref int handle, int deviceID,
            MidiOutProc proc, int instance, int flags);

        // Represents the method that handles messages from Windows.
        public delegate void MidiOutProc(int handle, int msg, int instance, int param1, int param2);

        [DllImport("winmm.dll")]
        public static extern int midiOutClose(int handle);

        [DllImport("winmm.dll")]
        public static extern int midiOutGetErrorText(int errCode, StringBuilder message, int sizeOfMessage); //Fehler für midiOut-Methoden. Quelle: https://stackoverflow.com/questions/19446403/windows-midi-streaming-and-sysex

        public static string GetMidiOutErrorText(int errCode)
        {
            if (0 == errCode) return ""; //No error
            var sb = new StringBuilder(256); // MAXERRORLENGTH
            var s = 0 == midiOutGetErrorText(errCode, sb, sb.Capacity) ? sb.ToString() : String.Format("MIDI Error {0}.", errCode);
            return s;
        }

        //Das geht nur, wenn beim MusicMachine-Projekt in der csproj-Datei folgender Eintrag steht: <PlatformTarget>x86</PlatformTarget>
        public static string PlayTestTone()
        {
            int handle = 0;
            string err1 = MidiDllWraper.GetMidiOutErrorText(MidiDllWraper.midiOutOpen(ref handle, 0, null, 0, 0));
            string err2 = MidiDllWraper.GetMidiOutErrorText(MidiDllWraper.midiOutShortMsg(handle, 0x000003C0));//00-00 Nor Used | 03 = Instrument 3 | C = Program Change-Event | 0 = Channel 0
            string err3 = MidiDllWraper.GetMidiOutErrorText(MidiDllWraper.midiOutShortMsg(handle, 0x00403C90));//00-Not Used | 40 Speed to Press | 3c 3. Note C | 9 = Press Down | 0 = Channel 0
            System.Threading.Thread.Sleep(1000);
            string err4 = MidiDllWraper.GetMidiOutErrorText(MidiDllWraper.midiOutShortMsg(handle, 0x00003C90));//00 Not Used |00 (Velocity)Key-Up |3C Note 3C | 90 = Key Up (Eigentlich ist NoteOff 8 aber wenn man NoteOn mit Velocity=0 macht, entspricht das ein NoteOff
            string err5 = MidiDllWraper.GetMidiOutErrorText(MidiDllWraper.midiOutClose(handle));

            return string.Join("\n", new string[] { err1, err2, err3, err4, err5 });
        }
    }

    /// <summary>
    /// Represents MIDI output device capabilities.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MidiOutCaps
    {
        #region MidiOutCaps Members

        /// <summary>
        /// Manufacturer identifier of the device driver for the Midi output 
        /// device. 
        /// </summary>
        public short mid;

        /// <summary>
        /// Product identifier of the Midi output device. 
        /// </summary>
        public short pid;

        /// <summary>
        /// Version number of the device driver for the Midi output device. The 
        /// high-order byte is the major version number, and the low-order byte 
        /// is the minor version number. 
        /// </summary>
        public int driverVersion;

        /// <summary>
        /// Product name.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string name;

        /// <summary>
        /// Flags describing the type of the Midi output device. 
        /// </summary>
        public short technology;

        /// <summary>
        /// Number of voices supported by an internal synthesizer device. If 
        /// the device is a port, this member is not meaningful and is set 
        /// to 0. 
        /// </summary>
        public short voices;

        /// <summary>
        /// Maximum number of simultaneous notes that can be played by an 
        /// internal synthesizer device. If the device is a port, this member 
        /// is not meaningful and is set to 0. 
        /// </summary>
        public short notes;

        /// <summary>
        /// Channels that an internal synthesizer device responds to, where the 
        /// least significant bit refers to channel 0 and the most significant 
        /// bit to channel 15. Port devices that transmit on all channels set 
        /// this member to 0xFFFF. 
        /// </summary>
        public short channelMask;

        /// <summary>
        /// Optional functionality supported by the device. 
        /// </summary>
        public int support;

        #endregion
    }
}
