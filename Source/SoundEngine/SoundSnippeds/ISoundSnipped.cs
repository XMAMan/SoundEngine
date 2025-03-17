using System;
using WaveMaker.KeyboardComponents;

namespace SoundEngine.SoundSnippeds
{
    public interface ISoundSnipped
    {
        bool IsRunning { get; }
        Action<bool> IsRunningChanged { get; set; }
        void Play();
        void Stop();
        float Volume { get; set; }
    }

    //Menge von Samples (Aus mp3/wma/wav oder .music-Datei)
    public interface IAudioFileSnipped : ISoundSnipped
    {
        void Reset(); //Springe zum Anfang zurück
        bool AutoLoop { get; set; } //Soll am Ende automatisch zum Anfang gesprungen werden?
    }

    public interface IFrequenceToneSnipped : ISoundSnipped
    {
        float Frequency { get; set; } //Für Synthi-Töne
        float Pitch { get; set; } //Für Audio-File-Töne
        Synthesizer Synthesizer { get; }
    }
}
