using NAudio.Lame;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using WaveMaker;

namespace NAudioWaveMaker
{
    public class AudioFileHandler : IAudioFileHandler
    {
        public AudioFileHandler()
        {
            
        }

        //https://markheath.net/post/how-to-use-wavefilewriter
        private void Beispiel1_WaveFileErzeugen()
        {
            var waveFileWriter = new WaveFileWriter("output.wav", new WaveFormat(44100 / 2, 2));

            float amplitude = 0.25f;
            float frequency = 1000;

            for (int n = 0; n < waveFileWriter.WaveFormat.SampleRate; n++)
            {
                float sample = (float)(amplitude * Math.Sin((2 * Math.PI * n * frequency) / waveFileWriter.WaveFormat.SampleRate));
                waveFileWriter.WriteSample(sample);
            }

            waveFileWriter.Flush();
            waveFileWriter.Dispose();
        }

        //https://csharp.hotexamples.com/de/site/file?hash=0x40b980ea851092b8648e04418983b1c545a9998a3b7059fdf9d332ffb95ea658&fullName=PawellsMusicEditor/WavFileUtils.cs&project=Qder/MusicEditor
        private void Beipisel2_WaveFileNachMp3Konvertieren()
        {
            WaveFileReader rdr = new WaveFileReader("output.wav");
            using (var wtr = new LameMP3FileWriter("output.mp3", rdr.WaveFormat, 128))
            {
                rdr.CopyTo(wtr);
                rdr.Dispose();
                wtr.Dispose();
                return;
            }
        }

        private void Beispiel3_Mp3Erzeugen()
        {
            MemoryStream memory = new MemoryStream();
            var waveFileWriter = new WaveFileWriter(memory, new WaveFormat(44100 / 2, 2));

            float amplitude = 0.25f;
            float frequency = 1000;

            for (int n = 0; n < waveFileWriter.WaveFormat.SampleRate; n++)
            {
                float sample = (float)(amplitude * Math.Sin((2 * Math.PI * n * frequency) / waveFileWriter.WaveFormat.SampleRate));
                waveFileWriter.WriteSample(sample);
            }

            waveFileWriter.Flush();

            memory.Position = 0;

            WaveFileReader rdr = new WaveFileReader(memory);

            LameMP3FileWriter writer = new LameMP3FileWriter("output.mp3", waveFileWriter.WaveFormat, LAMEPreset.V4);
            rdr.CopyTo(writer);
            rdr.Dispose();
            writer.Dispose();
            waveFileWriter.Dispose();
        }

        //Erzeugt die Datei outFileName aus den ersten 'sampleCount' Samples des AudioStreams
        public void ExportAudioStreamToFile(float[] samples, int sampelRate, string outputFile)
        {
            float[] stereoSamples = MakeStereoFromMonoData(samples);

            if (outputFile.EndsWith(".wav"))
                ExportAudioStreamToWavFile(stereoSamples, sampelRate, outputFile);

            if (outputFile.EndsWith(".mp3"))
                ExportAudioStreamToMp3File(stereoSamples, sampelRate, outputFile);
        }

        private void ExportAudioStreamToWavFile(float[] samples, int sampelRate, string outputFile)
        {
            var waveFileWriter = new WaveFileWriter(outputFile, new WaveFormat(sampelRate, 2));

            waveFileWriter.WriteSamples(samples, 0, samples.Length);

            waveFileWriter.Flush();
            waveFileWriter.Dispose();
        }

        private void ExportAudioStreamToMp3File(float[] samples, int sampelRate, string outputFile)
        {
            MemoryStream memory = new MemoryStream();
            var waveFileWriter = new WaveFileWriter(memory, new WaveFormat(sampelRate, 2));

            waveFileWriter.WriteSamples(samples, 0, samples.Length);

            waveFileWriter.Flush();

            memory.Position = 0;

            WaveFileReader rdr = new WaveFileReader(memory);

            LameMP3FileWriter writer = new LameMP3FileWriter(outputFile, waveFileWriter.WaveFormat, LAMEPreset.V4);
            rdr.CopyTo(writer);
            rdr.Dispose();
            writer.Dispose();
            waveFileWriter.Dispose();
        }

        private float[] MakeStereoFromMonoData(float[] monoSamples)
        {
            float[] stereoSamples = new float[monoSamples.Length * 2];
            for (int i=0;i<monoSamples.Length;i++)
            {
                stereoSamples[i * 2 + 0] = monoSamples[i];
                stereoSamples[i * 2 + 1] = monoSamples[i];
            }
            return stereoSamples;
        }

        private float[] MakeMonoFromStereoData(float[] stereoSamples)
        {
            float[] monoSamples = new float[stereoSamples.Length / 2];
            for (int i = 0; i < monoSamples.Length; i++)
            {
                float f1 = stereoSamples[i * 2 + 0];
                float f2 = stereoSamples[i * 2 + 1];
                monoSamples[i] = (f1 + f2) / 2;
            }
            return monoSamples;
        }

        //outputSampleRate = Gewünschte Ausgabe-Samplerate
        public float[] GetSamplesFromAudioFile(string audioFile, int outputSampleRate) 
        {
            List<float> samples = new List<float>();
            using (var reader = new AudioFileReader(audioFile))
            {
                var resampler = new WdlResamplingSampleProvider(reader, outputSampleRate); //https://csharp.hotexamples.com/de/examples/NAudio.Wave/AudioFileReader/-/php-audiofilereader-class-examples.html

                float[] buffer = new float[resampler.WaveFormat.AverageBytesPerSecond];
                while (true)
                {
                    int read = resampler.Read(buffer, 0, buffer.Length);
                    if (read > 0)
                        samples.AddRange(buffer);
                    else
                        break;
                }
            }
            var timmedSamples = Trim(samples.ToArray());
            return MakeMonoFromStereoData(timmedSamples);
        }

        //Entferne führende und folgende Nullen
        private static float[] Trim(float[] samples)
        {
            int startIndex = 0;
            while (startIndex < samples.Length && samples[startIndex] == 0)
            {
                startIndex++;
            }

            int endIndex = samples.Length - 1;
            while (endIndex > startIndex && samples[endIndex] == 0)
            {
                endIndex--;
            }

            //Man muss hinten noch ein paar Nullen dranlassen, da der Ton sonst so klingt, als ob das Ende fehlt
            endIndex = Math.Min(endIndex + 5000, samples.Length - 1);

            float[] trimData = new float[endIndex - startIndex + 1];
            Array.Copy(samples, startIndex, trimData, 0, endIndex - startIndex + 1);
            return trimData;
        }

    }
}
