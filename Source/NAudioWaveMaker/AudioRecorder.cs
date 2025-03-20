using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using WaveMaker;

namespace NAudioWaveMaker
{
    //https://swharden.com/csdv/audio/naudio/
    //https://markheath.net/post/how-to-record-and-play-audio-at-same
    public class AudioRecorder : IAudioRecorder, IDisposable, ISingleSampleProvider
    {
        private const string DefaultDevice = "Default";

        private NAudio.Wave.WaveInEvent waveIn; //Liefert die Audio-Daten vom Mikrofon als Byte-Array
        private BufferedWaveProvider buffered;  //Puffert die Daten als Byte-Array
        private ISampleProvider bufferedSampleProvider; //Liefert die Daten aus dem Puffer als Float-Array
        private NAudio.Wave.WaveFormat waveFormat;

        public int SampleRate { get; private set; }

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

        public bool IsRecording { get; private set; } = false;

        public AudioRecorder(int sampleRate) 
        {
            this.SampleRate = sampleRate;
            this.waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);

            UseDefaultDevice();
        }

        public string[] GetAvailableDevices()
        {
            List<string> availableMicrophones = new List<string>();
            for (int i = 0; i < NAudio.Wave.WaveIn.DeviceCount; i++)
            {
                var caps = NAudio.Wave.WaveIn.GetCapabilities(i);
                availableMicrophones.Add(caps.ProductName);
            }


            return availableMicrophones.ToArray();
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
            bool isRecordingOldValue = this.IsRecording;

            StopRecording();

            int deviceNumber = GetDeviceNumber(deviceName);

            this.waveIn = new NAudio.Wave.WaveInEvent
            {
                DeviceNumber = deviceNumber, // indicates which microphone to use
                WaveFormat = this.waveFormat,
                BufferMilliseconds = 20
            };
            waveIn.DataAvailable += WaveIn_DataAvailable;

            this.buffered = new BufferedWaveProvider(waveIn.WaveFormat);
            this.buffered.BufferDuration = TimeSpan.FromSeconds(2); // allow us to get 2 seconds behind
            this.buffered.DiscardOnBufferOverflow = true;
            this.bufferedSampleProvider = buffered.ToSampleProvider();

            if (isRecordingOldValue)
            {
                StartRecording();
            }
        }

        public void StartRecording()
        {
            if (this.selectedDevice == null)
            {
                return;
            }

            if (this.waveIn == null)
            {
                SwitchDevice(this.SelectedDevice);
            }

            if (this.waveIn != null)
            {
                waveIn.StartRecording();
                this.IsRecording = true;
            } 
        }

        public void StopRecording()
        {
            if (this.waveIn != null)
            {
                this.waveIn.StopRecording();
                this.waveIn = null;
            }

            this.IsRecording = false;
        }

        void WaveIn_DataAvailable(object sender, NAudio.Wave.WaveInEventArgs e)
        {
            buffered.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }

        public void Dispose()
        {
            if (this.waveIn != null)
            {
                this.waveIn.StopRecording();
                this.waveIn.Dispose();
            }
        }

        public float GetNextSample()
        {
            float[] outBuffer = new float[1];
            int readedSamples = this.bufferedSampleProvider.Read(outBuffer, 0, outBuffer.Length);
            return outBuffer[0];          
        }
    }
}
