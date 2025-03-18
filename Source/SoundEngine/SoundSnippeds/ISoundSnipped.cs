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

    public interface ISoundSnippedWithEndTrigger : ISoundSnipped
    {
        void Reset(); //Springe zum Anfang zurück
        bool AutoLoop { get; set; } //Soll am Ende automatisch zum Anfang gesprungen werden?
        Action EndTrigger { get; set; } //Wird aufgerufen, wenn das Ende erreicht wurde
    }

    public interface IMusicFileSnipped : ISoundSnippedWithEndTrigger
    {
        float KeyStrokeSpeed { get; set; } //1 = Spiele in normaler Geschwindigkeit, 2 = Doppelt so schnell, 0.5 = Halbe Geschwindigkeit
    }

    //Menge von Samples (Aus mp3/wma/wav oder .music-Datei)
    public interface IAudioFileSnipped : ISoundSnippedWithEndTrigger
    {
        float Pitch { get; set; } //Für Audio-File-Töne
        float Speed { get; set; } //Für Audio-File-Töne
        bool UseDelayEffekt { get; set; }
        bool UseHallEffekt { get; set; }
        bool UseGainEffekt { get; set; }
        float Gain { get; set; }
        bool UseVolumeLfo { get; set; }
        float VolumeLfoFrequency { get; set; }
    }

    public interface IFrequenceToneSnipped : ISoundSnipped
    {
        float Frequency { get; set; } //Für Synthi-Töne        
        Synthesizer Synthesizer { get; }
    }
}
