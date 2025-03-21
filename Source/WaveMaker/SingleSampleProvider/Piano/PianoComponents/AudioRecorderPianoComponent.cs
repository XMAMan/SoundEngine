namespace WaveMaker.KeyboardComponents
{
    public class AudioRecorderPianoComponent : IPianoComponent
    {
        public IAudioRecorder AudioRecorder { get; private set; }
        public AudioRecorderPianoComponent(IAudioRecorder audioRecorder)
        {
            this.AudioRecorder = audioRecorder;
        }

        //sorge dafür, dass wenn GetSample für ein SampleIndex mehrmals aufgerufen wird, dass es immer den gleichen Wert zurück gibt
        private int alreadyUsedIndex = -1;
        private float lastSample = 0;

        public float GetSample(KeySampleData data)
        {
            if (data.SampleIndex != this.alreadyUsedIndex)
            {
                this.alreadyUsedIndex = data.SampleIndex;
                this.lastSample = this.AudioRecorder.GetNextSample();
                return this.lastSample;
            }else
            {
                return this.lastSample;
            }            
        }
    }
}
