using System.Runtime.InteropServices;

namespace MidiPlayer
{
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
