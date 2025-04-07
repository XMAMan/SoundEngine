using WaveMaker;
using WaveMaker.KeyboardComponents;

namespace SoundEngine.SoundSnippeds
{
    public interface ISoundSnipped : ISingleSampleProvider, IDisposable
    {
        bool IsRunning { get; }
        Action<bool> IsRunningChanged { get; set; }
        void Play();
        void Stop(); //Bei ein FrequencyTone stoppt es alle laufenden Töne
        float Volume { get; set; }
        Action<ISoundSnipped> CopyWasCreated { get; set; }
        Action<ISoundSnipped> DisposeWasCalled { get; set; } //Wird aufgerufen, wenn Dispose aufgerufen wurde
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
        int KeyShift { get; set; } //Wie viele Okataven nach oben oder unten verschieben
        Synthesizer GetSynthesizer(int index);
        IMusicFileSnipped GetCopy();
    }

    public interface IAudioEffects
    {
        bool UseDelayEffect { get; set; }
        bool UseHallEffect { get; set; }
        bool UseGainEffect { get; set; }
        float Gain { get; set; }
        bool UsePitchEffect { get; set; }
        float PitchEffect { get; set; }
        bool UseVolumeLfo { get; set; }
        float VolumeLfoFrequency { get; set; }
    }

    //Menge von Samples (Aus mp3/wma/wav oder .music-Datei)
    public interface IAudioFileSnipped : ISoundSnippedWithEndTrigger, IAudioEffects
    {
        float Pitch { get; set; } //Für Audio-File-Töne
        float Speed { get; set; } //Für Audio-File-Töne
        IAudioFileSnipped GetCopy();
    }

    public interface IFrequenceToneSnipped : ISoundSnipped
    {
        float Frequency { get; set; } //Für Synthi-Töne        
        Synthesizer Synthesizer { get; }
        IFrequenceToneSnipped GetCopy();
    }

    public interface IAudioRecorderSnipped : ISoundSnipped, IAudioEffects
    {
        string[] GetAvailableDevices();
        string SelectedDevice { get; set; }
        void UseDefaultDevice();
        IAudioRecorderSnipped GetCopy();
    }
}

