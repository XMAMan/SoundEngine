using SoundEngine.SoundSnippeds;
using WaveMaker;

namespace SoundEngine
{
    public interface ISoundGenerator
    {
        int SampleRate { get; }
        float Volume { get; set; }
        string SelectedOutputDevice { get; set; }
        string[] GetAvailableOutputDevices();
        IAudioRecorderSnipped AudioRecorder { get; }
        IAudioFileWriter AudioFileWriter { get; }
        event EventHandler<float[]> AudioOutputCallback; //Wird zyklisch vom Timer gerufen, wenn er nach neuen Audiodaten fragt.

        IFrequenceToneSnipped AddFrequencyTone(string syntiFile);
        IMusicFileSnipped AddMusicFile(string musicFile);
        IAudioFileSnipped AddSoundFile(string audioFile);
        IFrequenceToneSnipped[] AddSynthSoundCollection(string musicFile);
        void Dispose();
    }
}