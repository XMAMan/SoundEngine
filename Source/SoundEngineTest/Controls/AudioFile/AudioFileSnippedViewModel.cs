using SoundEngine.SoundSnippeds;
using SoundEngineTest.ViewModel;

namespace SoundEngineTest.Controls.AudioFile
{
    public class AudioFileSnippedViewModel : SoundWithEndTrigger
    {
        private IAudioFileSnipped snipp;

        public AudioFileSnippedViewModel(IAudioFileSnipped snipp)
            : base(snipp)
        {
            this.snipp = snipp;
        }

        public float Pitch { get { return snipp.Pitch; } set { snipp.Pitch = value; } }
        public float Speed { get { return snipp.Speed; } set { snipp.Speed = value; } }
        public bool UseDelayEffect { get { return snipp.UseDelayEffect; } set { snipp.UseDelayEffect = value; } }
        public bool UseHallEffect { get { return snipp.UseHallEffect; } set { snipp.UseHallEffect = value; } }
        public bool UseGainEffect { get { return snipp.UseGainEffect; } set { snipp.UseGainEffect = value; } }
        public float Gain { get { return snipp.Gain; } set { snipp.Gain = value; } }
        public bool UsePitchEffect { get { return snipp.UsePitchEffect; } set { snipp.UsePitchEffect = value; } }
        public float PitchEffect { get { return snipp.PitchEffect; } set { snipp.PitchEffect = value; } }
        public bool UseVolumeLfo { get { return snipp.UseVolumeLfo; } set { snipp.UseVolumeLfo = value; } }
        public float VolumeLfoFrequency { get { return snipp.VolumeLfoFrequency; } set { snipp.VolumeLfoFrequency = value; } }
    }
}
