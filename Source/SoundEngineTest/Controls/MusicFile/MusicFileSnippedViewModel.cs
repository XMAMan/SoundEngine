using SoundEngine.SoundSnippeds;
using SoundEngineTest.ViewModel;

namespace SoundEngineTest.Controls.MusicFile
{
    public class MusicFileSnippedViewModel : SoundWithEndTrigger
    {
        private IMusicFileSnipped snipp;
        public MusicFileSnippedViewModel(IMusicFileSnipped snip)
            : base(snip)
        {
            snipp = snip;
        }
        public float KeyStrokeSpeed { get { return snipp.KeyStrokeSpeed; } set { snipp.KeyStrokeSpeed = value; } }
        public int KeyShift { get { return snipp.KeyShift; } set { snipp.KeyShift = value; } }
    }
}
