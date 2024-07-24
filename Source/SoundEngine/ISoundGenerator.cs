using SoundEngine.SoundSnippeds;

namespace SoundEngine
{
    public interface ISoundGenerator
    {
        int SampleRate { get; }
        float Volume { get; set; }

        IFrequenceToneSnipped AddFrequencyTone();
        IFrequenceToneSnipped AddFrequencyTone(string syntiFile);
        IAudioFileSnipped AddMusicFile(string musicFile);
        IAudioFileSnipped AddSoundFile(string audioFile);
        IFrequenceToneSnipped[] AddSynthSoundCollection(string musicFile);
        void Dispose();
        IFrequenceToneSnipped[] GetAllFrequenceTones();
    }
}