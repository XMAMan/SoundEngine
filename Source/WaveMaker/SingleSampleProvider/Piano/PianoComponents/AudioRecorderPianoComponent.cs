namespace WaveMaker.KeyboardComponents
{
    public class AudioRecorderPianoComponent : IPianoComponent
    {
        public IAudioRecorder AudioRecorder { get; private set; }
        public AudioRecorderPianoComponent(IAudioRecorder audioRecorder)
        {
            this.AudioRecorder = audioRecorder;
        }
        public float GetSample(KeySampleData data)
        {
            return this.AudioRecorder.GetNextSample();
        }
    }
}
