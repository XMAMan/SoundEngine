using NAudio;
using NAudio.Wave;
using System.Runtime.InteropServices;
using WaveMaker;

//https://csharp.hotexamples.com/de/examples/NAudio.Wave/AudioFileReader/-/php-audiofilereader-class-examples.html -> NAudio-Beispiele

namespace NAudioWaveMaker
{
    public class AudioPlayer : IAudioPlayer, IDisposable
    {
        private const string DefaultDevice = "Default";

        private WaveOutEvent driverOut;
        private ISampleProvider sampleProvider;

        public event EventHandler<float[]> AudioOutputCallback //Wird zyklisch vom Timer gerufen, wenn er nach neuen Audiodaten fragt
        {
            add
            {
                if (this.sampleProvider != null)
                {
                    ((SampleProviderFromSingleSampleProvider)this.sampleProvider).AudioOutputCallback += value;
                }
            }
            remove
            {
                if (this.sampleProvider != null)
                {
                    ((SampleProviderFromSingleSampleProvider)this.sampleProvider).AudioOutputCallback -= value;
                }
            }
        }

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
            for (int idx = 0; idx < WaveInterop.waveOutGetNumDevs(); ++idx)
            {
                string devName = GetCapabilities(idx).ProductName;
                deviceNames.Add(devName);
            }

            return deviceNames.ToArray();
        }

        private static WaveOutCapabilities GetCapabilities(int devNumber)
        {
            WaveOutCapabilities waveOutCaps = default(WaveOutCapabilities);
            int waveOutCapsSize = Marshal.SizeOf(waveOutCaps);
            MmException.Try(WaveInterop.waveOutGetDevCaps((IntPtr)devNumber, out waveOutCaps, waveOutCapsSize), "waveOutGetDevCaps");
            return waveOutCaps;
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

            this.driverOut = new WaveOutEvent();
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
        private TaskScheduler uiContext; //Wird benötigt, um Exceptions vom Audio-Timer-Thread in den GUI-Thread zu bringen

        public WaveFormat WaveFormat { get; private set; }

        public event EventHandler<float[]> AudioOutputCallback; //Wird zyklisch vom Timer gerufen, wenn er nach neuen Audiodaten fragt

        public SampleProviderFromSingleSampleProvider(ISingleSampleProvider audioCallback)
        {
            this.audioCallback = audioCallback;
            this.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(audioCallback.SampleRate, 2); //SampleRate = 44100 / 2
            this.uiContext = TaskScheduler.FromCurrentSynchronizationContext();
        }

        private List<float> outputSamples = new List<float>();

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

                    outputSamples.Add(sampleValue); //Wird für AudioOutputCallback benötigt

                    for (int i = 0; i < WaveFormat.Channels; i++)
                    {
                        buffer[outIndex++] = (float)(sampleValue);
                    }                    
                }
                catch (Exception ex)
                {
                    Task.Factory.StartNew(() =>
                    {
                        throw new Exception("AudioPlayer", ex);
                    }, System.Threading.CancellationToken.None, TaskCreationOptions.None, this.uiContext);
                }
                
                this.sampleIndex++;
            }

            AudioOutputCallback?.Invoke(this, outputSamples.ToArray());
            outputSamples.Clear();

            return count;
        }
    }
}
