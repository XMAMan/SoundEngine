using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using WaveMaker;

//https://csharp.hotexamples.com/de/examples/NAudio.Wave/AudioFileReader/-/php-audiofilereader-class-examples.html -> NAudio-Beispiele

namespace NAudioWaveMaker
{
    public class AudioPlayer : IAudioPlayer, IDisposable
    {
        private const string DefaultDevice = "Default";

        private WaveOut driverOut;
        private ISampleProvider sampleProvider;

        public AudioPlayer(ISingleSampleProvider audioCallback)
        {
            this.sampleProvider = new SampleProviderFromSingleSampleProvider(audioCallback);

            UseDefaultDevice();
        }

        private string selectedDevice = null;
        public string SelectedDevice
        {
            get => this.selectedDevice;
            set
            {
                this.selectedDevice = value;
                SwitchDevice(value);
            }
        }

        public void UseDefaultDevice()
        {
            this.SelectedDevice = DefaultDevice;
        }

        public bool IsPlaying { get; private set; } = false;

        public string[] GetAvailableDevices()
        {
            List<string> deviceNames = new List<string>();
            for (int idx = 0; idx < NAudio.Wave.WaveOut.DeviceCount; ++idx)
            {
                string devName = NAudio.Wave.WaveOut.GetCapabilities(idx).ProductName;
                deviceNames.Add(devName);
            }

            return deviceNames.ToArray();
        }

        private int GetDeviceNumber(string deviceName)
        {
            if (deviceName == DefaultDevice) return -1;

            string[] devices = GetAvailableDevices();
            int index = devices.ToList().IndexOf(deviceName);
            if (index == -1) throw new ArgumentException($"Unknown device name '{deviceName}'");
            return index;
        }

        private void SwitchDevice(string deviceName)
        {
            bool isPlayingOldValue = this.IsPlaying;

            StopPlaying();

            int deviceNumber = GetDeviceNumber(deviceName);

            this.driverOut = new WaveOut();
            this.driverOut.DeviceNumber = deviceNumber;
            this.driverOut.DesiredLatency = 100; //So viel Zeit wartet NAudio, bevor es meine Samples dann zur Soundkarte schickt
            this.driverOut.Init(this.sampleProvider);

            if (isPlayingOldValue)
            {
                StartPlaying();
            }
        }

        public void StartPlaying()
        {
            if (this.selectedDevice == null)
            {
                return;
            }

            if (this.driverOut == null)
            {
                SwitchDevice(this.SelectedDevice);
            }

            if (this.driverOut != null)
            {
                this.driverOut.Play();
                this.IsPlaying = true;
            }
        }

        public void StopPlaying()
        {
            if (this.driverOut != null)
            {
                this.driverOut.Stop();
                this.driverOut = null;
            }

            this.IsPlaying = false;
        }

        public void Dispose() //Wenn das im Destruktor schreibe, dann schimpft NAudio rum, obwohl diese Methode gerufen wurde
        {
            if (driverOut != null)
                driverOut.Stop();

            if (driverOut != null)
            {
                driverOut.Stop();
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
