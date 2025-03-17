using System.IO;
using System.Linq;

namespace WaveMaker.KeyboardComponents
{
    public class SynthesizerData
    {        
        public bool UseFrequencyLfo { get; set; } = false;
        public float FrequencyLfoFrequenc { get; set; } = 5;
        public float FrequencyLfoAmplitude { get; set; } = 0.8f;
        public bool UseFrequenceRamp { get; set; } = false;
        public float FrequencyRampFactor { get; set; } = -0.3f;
        public int RampStepFactor { get; set; } = 0;
        public bool UsePulsewidthLfo { get; set; } = false;
        public float PulsewidthLfoFrequence { get; set; } = 5;
        public float PulsewidthLfoAmplitude { get; set; } = 0.2f;
        public SignalType OsziType { get; set; } = SignalType.Rectangle;
        public bool UseAccordEffekt { get; set; } = false;
        public float PusleWidth { get; set; } = 0.5f;
        public int OsziCount { get; set; } = 2;
        public float Pitch { get; set; } = 3;
        public float SubOszVolume { get; set; } = 0;
        public bool UseAmplitudeLfo { get; set; } = false;
        public float AmplitudeLfoFrequenc { get; set; } = 5;
        public float AmplitudeLfoAmplitude { get; set; } = 0.1f;        
        public float AmplitudeLfoPulseWidth { get; set; } = 0.9f;
        public string AudioFileName { get; set; } = "";
        public float LeftAudioFilePosition { get; set; } = 0;
        public float RightAudioFilePosition { get; set; } = 0;
        public bool UseDataFromAudioFileInsteadFromOszi { get; set; } = false;
        public float AudioFileGain { get; set; } = 100;
        public float AudioFilePitch { get; set; } = 1;
        public bool IsLowPassEnabled { get; set; } = false;
        public float CutOffFrequence { get; set; } = 1;
        public float Resonance { get; set; } = 0.2f;
        public float AttackTimeInMs { get; set; } = 50;
        public float DecayTimeInMs { get; set; } = 70;
        public float SustainVolume { get; set; } = 0.9f;
        public float ReleaseTimeInMs { get; set; } = 250;
        public bool UseDelayEffekt { get; set; } = false;
        public bool UseHallEffekt { get; set; } = false;
        public bool UseGainEffekt { get; set; } = false;
        public float Gain { get; set; } = 7;
        public bool UseVolumeLfo { get; set; } = false;
        public float VolumeLfoFrequency { get; set; } = 5;
    }

    //Ein Synthesizer ist eine Menge von OscilatorWithLfo, dessen Frequenz und Amplitude über ein LFO gesteuert wird. Die per LFO gesteuerte Frequenz geht 
    //danach über die Frequenz-Rampe (Auch noch Bestandteil vom OscilatorWithLfo). Nach diesen so gesteuerten Ossilatoren vereint der Mixer die Oszis und 
    //danach kommt dann der Tiefpass-Filter und danach die Hüllkurve.
    //[OscilatorWithLfo] -> [MultiOscillator] -> [Filter] -> [AdsrEnvelope]
    public class Synthesizer : IPianoComponent
    {
        public OscilatorWithLfo Oscilator { get; private set; }
        public MultiOscillator MultiOscillator { get; private set; }
        public Mixer OscWithSubOscMixer { get; private set; }
        public AudioFile AudioFile { get; private set; } //Kann anstelle der gemixten Oszilatoren genommen werden
        public Switch OsziAudioFileSwitch { get; private set; }
       
        public Filter LowPass { get; private set; }
        public AdsrEnvelope AdsrEnvelope { get; private set; }
        public DelayEffect DelayEffect { get; private set; }        
        public HallEffect HallEffect { get; private set; }
        public GainEffect GainEffect { get; private set; }
        public VolumeLfo VolumeLfo { get; private set; }

        private IPianoStopKeyHandler[] stopKeyHandler; //Sagen, wie lange nach dem Release-Key-Signal noch der Ton weiter geht 

        public Synthesizer(int sampleRate)
        {
            this.Oscilator = new OscilatorWithLfo(sampleRate);
            this.MultiOscillator = new MultiOscillator(this.Oscilator);
            this.OscWithSubOscMixer = new Mixer(this.MultiOscillator, new SubOscilator(sampleRate) { OsziType = SignalType.SawTooth }) { VolumeB = 0 };
            this.AudioFile = new AudioFile(sampleRate);
            this.OsziAudioFileSwitch = new Switch(this.OscWithSubOscMixer, this.AudioFile);
            //this.LowPass = new Filter(this.MultiOscillator, FilterType.LowPass, sampleRate); //Nehme Daten vom Oszi
            //this.LowPass = new Filter(this.AudioFile, FilterType.LowPass, sampleRate);         //Nehme Daten aus AudioFile
            this.LowPass = new Filter(this.OsziAudioFileSwitch, FilterType.LowPass, sampleRate);//Nehme Daten aus Weiche
            this.AdsrEnvelope = new AdsrEnvelope(this.LowPass, sampleRate);
            this.DelayEffect = new DelayEffect(this.AdsrEnvelope, sampleRate);
            this.HallEffect = new HallEffect(this.DelayEffect, sampleRate);
            this.GainEffect = new GainEffect(this.HallEffect);
            this.VolumeLfo = new VolumeLfo(this.GainEffect, sampleRate);

            this.stopKeyHandler = new IPianoStopKeyHandler[]
            {
                this.AdsrEnvelope,
                this.DelayEffect,
                this.HallEffect
            };
        }

        public void SetAllSettings(SynthesizerData data, IAudioFileReader audioFileReader, string searchDirectoryForAudioFiles, int sampleRate)
        {
            this.UseFrequencyLfo = data.UseFrequencyLfo;
            this.FrequencyLfoFrequenc = data.FrequencyLfoFrequenc;
            this.FrequencyLfoAmplitude = data.FrequencyLfoAmplitude;
            this.UseFrequenceRamp = data.UseFrequenceRamp;
            this.FrequencyRampFactor = data.FrequencyRampFactor;
            this.RampStepFactor = data.RampStepFactor;
            this.UsePulsewidthLfo = data.UsePulsewidthLfo;
            this.PulsewidthLfoFrequence = data.PulsewidthLfoFrequence;
            this.PulsewidthLfoAmplitude = data.PulsewidthLfoAmplitude;
            this.OsziType = data.OsziType;
            this.UseAccordEffekt = data.UseAccordEffekt;
            this.PusleWidth = data.PusleWidth;
            this.OsziCount = data.OsziCount;
            this.Pitch = data.Pitch;
            this.SubOszVolume = data.SubOszVolume;
            this.UseAmplitudeLfo = data.UseAmplitudeLfo;
            this.AmplitudeLfoFrequenc = data.AmplitudeLfoFrequenc;
            this.AmplitudeLfoAmplitude = data.AmplitudeLfoAmplitude;
            this.AmplitudeLfoPulseWidth = data.AmplitudeLfoPulseWidth;
            this.AudioFileName = data.AudioFileName;
            string absolutPath = searchDirectoryForAudioFiles + "\\" + data.AudioFileName;
            if (File.Exists(absolutPath))
            {
                this.AudioFileData = audioFileReader.GetSamplesFromAudioFile(absolutPath, sampleRate);
            }
            this.LeftAudioFilePosition = data.LeftAudioFilePosition;
            this.RightAudioFilePosition = data.RightAudioFilePosition;
            this.UseDataFromAudioFileInsteadFromOszi = data.UseDataFromAudioFileInsteadFromOszi;            
            this.AudioFilePitch = data.AudioFilePitch;
            this.IsLowPassEnabled = data.IsLowPassEnabled;
            this.CutOffFrequence = data.CutOffFrequence;
            this.Resonance = data.Resonance;
            this.AttackTimeInMs = data.AttackTimeInMs;
            this.DecayTimeInMs = data.DecayTimeInMs;
            this.SustainVolume = data.SustainVolume;
            this.ReleaseTimeInMs = data.ReleaseTimeInMs;
            this.UseDelayEffekt = data.UseDelayEffekt;
            this.UseHallEffekt = data.UseHallEffekt;
            this.UseGainEffekt = data.UseGainEffekt;
            this.GainEffect.Gain = data.Gain;
            this.UseVolumeLfo = data.UseVolumeLfo;
            this.VolumeLfo.Frequency = data.VolumeLfoFrequency;
        }

        public SynthesizerData GetAllSettings()
        {
            return new SynthesizerData()
            {
                UseFrequencyLfo = this.UseFrequencyLfo,
                FrequencyLfoFrequenc = this.FrequencyLfoFrequenc,
                FrequencyLfoAmplitude = this.FrequencyLfoAmplitude,
                UseFrequenceRamp = this.UseFrequenceRamp,
                FrequencyRampFactor = this.FrequencyRampFactor,
                RampStepFactor = this.RampStepFactor,
                UsePulsewidthLfo = this.UsePulsewidthLfo,
                PulsewidthLfoFrequence = this.PulsewidthLfoFrequence,
                PulsewidthLfoAmplitude = this.PulsewidthLfoAmplitude,
                OsziType = this.OsziType,
                UseAccordEffekt = this.UseAccordEffekt,
                PusleWidth = this.PusleWidth,
                OsziCount = this.OsziCount,
                Pitch = this.Pitch,
                SubOszVolume = this.SubOszVolume,
                UseAmplitudeLfo = this.UseAmplitudeLfo,
                AmplitudeLfoFrequenc = this.AmplitudeLfoFrequenc,
                AmplitudeLfoAmplitude = this.AmplitudeLfoAmplitude,
                AmplitudeLfoPulseWidth = this.AmplitudeLfoPulseWidth,
                AudioFileName = this.AudioFileName,
                LeftAudioFilePosition = this.LeftAudioFilePosition,
                RightAudioFilePosition = this.RightAudioFilePosition,
                UseDataFromAudioFileInsteadFromOszi = this.UseDataFromAudioFileInsteadFromOszi,               
                AudioFilePitch = this.AudioFilePitch,
                IsLowPassEnabled = this.IsLowPassEnabled,
                CutOffFrequence = this.CutOffFrequence,
                Resonance = this.Resonance,
                AttackTimeInMs = this.AttackTimeInMs,
                DecayTimeInMs = this.DecayTimeInMs,
                SustainVolume = this.SustainVolume,
                ReleaseTimeInMs = this.ReleaseTimeInMs,
                UseDelayEffekt = this.UseDelayEffekt,
                UseHallEffekt = this.UseHallEffekt,
                UseGainEffekt = this.GainEffect.IsEnabled,
                Gain = this.GainEffect.Gain,
                UseVolumeLfo = this.VolumeLfo.IsEnabled,
                VolumeLfoFrequency = this.VolumeLfo.Frequency
            };
        }

        public int GetMaxStopIndexTime()
        {
            return this.stopKeyHandler.Max(x => x.IsEnabled ? x.StopIndexTime : 0);
        }

        public float GetSample(KeySampleData data)
        {
            return this.VolumeLfo.GetSample(data);
        }

        
        public bool UseFrequencyLfo { get { return this.Oscilator.UseFrequencyLfo; } set { this.Oscilator.UseFrequencyLfo = value; } }
        public float FrequencyLfoFrequenc { get { return this.Oscilator.FrequencyLfoFrequenc; } set { this.Oscilator.FrequencyLfoFrequenc = value; } }
        public float FrequencyLfoAmplitude { get { return this.Oscilator.FrequencyLfoAmplitude; } set { this.Oscilator.FrequencyLfoAmplitude = value; } }
        
        public bool UseFrequenceRamp { get { return this.Oscilator.UseFrequenceRamp; } set { this.Oscilator.UseFrequenceRamp = value; } }
        public float FrequencyRampFactor { get { return this.Oscilator.FrequencyRampFactor; } set { this.Oscilator.FrequencyRampFactor = value; } }
        public int RampStepFactor { get { return this.Oscilator.RampStepFactor; } set { this.Oscilator.RampStepFactor = value; } }

        public bool UsePulsewidthLfo { get { return this.Oscilator.UsePulsewidthLfo; } set { this.Oscilator.UsePulsewidthLfo = value; } }
        public float PulsewidthLfoFrequence { get { return this.Oscilator.PulsewidthLfoFrequence; } set { this.Oscilator.PulsewidthLfoFrequence = value; } }
        public float PulsewidthLfoAmplitude { get { return this.Oscilator.PulsewidthLfoAmplitude; } set { this.Oscilator.PulsewidthLfoAmplitude = value; } }


        public SignalType OsziType { get { return this.Oscilator.OsziType; } set { this.Oscilator.OsziType = value; } }
        public bool UseAccordEffekt { get { return this.Oscilator.UseAccordEffekt; } set { this.Oscilator.UseAccordEffekt = value; } }
        public float PusleWidth { get { return this.Oscilator.PusleWidth; } set { this.Oscilator.PusleWidth = value; } }
        public int OsziCount { get { return this.MultiOscillator.OsziCount; } set { this.MultiOscillator.OsziCount = value; } }
        public float Pitch { get { return this.MultiOscillator.Pitch; } set { this.MultiOscillator.Pitch = value; } }
        public float SubOszVolume { get { return this.OscWithSubOscMixer.VolumeB; } set { this.OscWithSubOscMixer.VolumeB = value; } }
        public bool UseAmplitudeLfo { get { return this.Oscilator.UseAmplitudeLfo; } set { this.Oscilator.UseAmplitudeLfo = value; } }
        public float AmplitudeLfoFrequenc { get { return this.Oscilator.AmplitudeLfoFrequenc; } set { this.Oscilator.AmplitudeLfoFrequenc = value; } }
        public float AmplitudeLfoAmplitude { get { return this.Oscilator.AmplitudeLfoAmplitude; } set { this.Oscilator.AmplitudeLfoAmplitude = value; } }
        public float AmplitudeLfoPulseWidth { get { return this.Oscilator.AmplitudeLfoPulseWidth; } set { this.Oscilator.AmplitudeLfoPulseWidth = value; } }
        

        public string AudioFileName { get { return this.AudioFile.AudioFileName; } set { this.AudioFile.AudioFileName = value; } }
        public float[] AudioFileData { get { return this.AudioFile.SampleData; } set { this.AudioFile.SampleData = value; } }
        public float LeftAudioFilePosition { get { return this.AudioFile.LeftPositionInMilliseconds; } set { this.AudioFile.LeftPositionInMilliseconds = value; } }
        public float RightAudioFilePosition { get { return this.AudioFile.RightPositionInMilliseconds; } set { this.AudioFile.RightPositionInMilliseconds = value; } }
        public bool UseDataFromAudioFileInsteadFromOszi { get { return this.OsziAudioFileSwitch.SwitchValue == Switch.SwitchValues.B; } set { this.OsziAudioFileSwitch.SwitchValue = value ? Switch.SwitchValues.B : Switch.SwitchValues.A; } }
        public float AudioFilePitch { get { return this.AudioFile.Pitch; } set { this.AudioFile.Pitch = value; } }

        public bool IsLowPassEnabled { get { return this.LowPass.IsEnabled; } set { this.LowPass.IsEnabled = value; } }
        public float CutOffFrequence { get { return this.LowPass.CutOffFrequence; } set { this.LowPass.CutOffFrequence = value; } }
        public float Resonance { get { return this.LowPass.Resonance; } set { this.LowPass.Resonance = value; } }
        

        public float AttackTimeInMs { get { return this.AdsrEnvelope.AttackTimeInMs; } set { this.AdsrEnvelope.AttackTimeInMs = value; } }
        public float DecayTimeInMs { get { return this.AdsrEnvelope.DecayTimeInMs; } set { this.AdsrEnvelope.DecayTimeInMs = value; } }
        public float SustainVolume { get { return this.AdsrEnvelope.SustainVolume; } set { this.AdsrEnvelope.SustainVolume = value; } }
        public float ReleaseTimeInMs { get { return this.AdsrEnvelope.ReleaseTimeInMs; } set { this.AdsrEnvelope.ReleaseTimeInMs = value; } }
        

        public bool UseDelayEffekt { get { return this.DelayEffect.IsEnabled; } set { this.DelayEffect.IsEnabled = value; } }
        public bool UseHallEffekt { get { return this.HallEffect.IsEnabled; } set { this.HallEffect.IsEnabled = value; } }
        public bool UseGainEffekt { get { return this.GainEffect.IsEnabled; } set { this.GainEffect.IsEnabled = value; } }
        public float Gain { get { return this.GainEffect.Gain; } set { this.GainEffect.Gain = value; } }
        public bool UseVolumeLfo { get { return this.VolumeLfo.IsEnabled; } set { this.VolumeLfo.IsEnabled = value; } }
        public float VolumeLfoFrequency { get { return this.VolumeLfo.Frequency; } set { this.VolumeLfo.Frequency = value; } }
    }
}


