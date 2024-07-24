using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveMaker;

//https://csharp.hotexamples.com/de/examples/NAudio.Wave/AudioFileReader/-/php-audiofilereader-class-examples.html -> NAudio-Beispiele

namespace NAudioWaveMaker
{
    public class WaveMaker : IWaveMaker, IDisposable
    {
        private WaveOut driverOut;
        private ISampleProvider sampleProvider;

        public WaveMaker(ISingleSampleProvider audioCallback)
        {
            this.sampleProvider = new SampleProviderFromSingleSampleProvider(audioCallback);
        }

        public void StartPlaying()
        {            
            this.driverOut = new WaveOut();
            this.driverOut.DesiredLatency = 100; //So viel Zeit wartet NAudio, bevor es meine Samples dann zur Soundkarte schickt
            this.driverOut.Init(this.sampleProvider);

            this.driverOut.Play();
        }

        public void Dispose() //Wenn das im Destruktor schreibe, dann schimpft NAudio rum, obwohl diese Methode gerufen wurde
        {
            if (driverOut != null)
                driverOut.Stop();

            if (driverOut != null)
            {
                driverOut.Dispose();
            }
        }
    }

    class SampleProviderFromSingleSampleProvider : ISampleProvider
    {
        private int sampleIndex = 0;
        private ISingleSampleProvider audioCallback;
        public WaveFormat WaveFormat { get; private set; }

        public SampleProviderFromSingleSampleProvider(ISingleSampleProvider audioCallback)
        {
            this.audioCallback = audioCallback;
            this.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(audioCallback.SampleRate, 2); //SampleRate = 44100 / 2
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int outIndex = offset;


            for (int sampleCount = 0; sampleCount < count / WaveFormat.Channels; sampleCount++)
            {
                try
                {
                    float sampleValue = this.audioCallback.GetNextSample();
                    if (sampleValue > 1) sampleValue = 1;
                    if (sampleValue < -1) sampleValue = -1;
                    for (int i = 0; i < WaveFormat.Channels; i++)
                    {
                        buffer[outIndex++] = (float)(sampleValue);
                    }
                }
                catch (Exception ex)
                {
                    string error = ex.ToString();
                }
                
                this.sampleIndex++;
            }

            return count;
        }
    }
}
