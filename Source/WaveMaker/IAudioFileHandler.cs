namespace WaveMaker
{
    public interface IAudioFileHandler : IAudioFileReader, IAudioFileWriter
    {
    }

    public interface IAudioFileReader
    {
        float[] GetSamplesFromAudioFile(string audioFile, int outputSampleRate); //outputSampleRate = Gewünschte Ausgabe-Samplerate
    }

    public interface IAudioFileWriter
    {
        void ExportAudioStreamToFile(float[] samples, int sampelRate, string outFileName); //Erzeugt die Datei outFileName aus den samples, welche mit der sampelRate aufgenommen wurden
    }

}
