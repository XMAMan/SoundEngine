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
        public bool UseDelayEffekt { get { return snipp.UseDelayEffekt; } set { snipp.UseDelayEffekt = value; } }
        public bool UseHallEffekt { get { return snipp.UseHallEffekt; } set { snipp.UseHallEffekt = value; } }
        public bool UseGainEffekt { get { return snipp.UseGainEffekt; } set { snipp.UseGainEffekt = value; } }
        public float Gain { get { return snipp.Gain; } set { snipp.Gain = value; } }
        public bool UseVolumeLfo { get { return snipp.UseVolumeLfo; } set { snipp.UseVolumeLfo = value; } }
        public float VolumeLfoFrequency { get { return snipp.VolumeLfoFrequency; } set { snipp.VolumeLfoFrequency = value; } }
    }
}
