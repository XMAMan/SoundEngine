using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
