namespace WaveMaker
{
    //Liefert den Audio-Strom. Könnte man also auch als IAudioSampleProvider bezeichnen
    public interface ISingleSampleProvider
    {
        int SampleRate { get; }
        float GetNextSample();
    }
}
