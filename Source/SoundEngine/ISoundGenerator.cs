using SoundEngine.SoundSnippeds;

namespace SoundEngine
{
    public interface ISoundGenerator
    {
        int SampleRate { get; }
        float Volume { get; set; }
        string SelectedOutputDevice { get; set; }
        string[] GetAvailableOutputDevices();
        IAudioRecorderSnipped AudioRecorderSnipped { get; }

        IFrequenceToneSnipped AddFrequencyTone();
        IFrequenceToneSnipped AddFrequencyTone(string syntiFile);
        IMusicFileSnipped AddMusicFile(string musicFile);
        IAudioFileSnipped AddSoundFile(string audioFile);
        IFrequenceToneSnipped[] AddSynthSoundCollection(string musicFile);
        void Dispose();
    }
}