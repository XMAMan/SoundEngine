using WaveMaker.SingleSampleProvider.Piano.PianoComponents;

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
        public SignalType OsciType { get; set; } = SignalType.Rectangle;
        public bool UseAccordEffect { get; set; } = false;
        public float PusleWidth { get; set; } = 0.5f;
        public int OsciCount { get; set; } = 2;
        public float MultiOsciPitch { get; set; } = 3;
        public float SubOszVolume { get; set; } = 0;
        public bool UseAmplitudeLfo { get; set; } = false;
        public float AmplitudeLfoFrequenc { get; set; } = 5;
        public float AmplitudeLfoAmplitude { get; set; } = 0.1f;        
        public float AmplitudeLfoPulseWidth { get; set; } = 0.9f;
        public string AudioFileName { get; set; } = "";
        public float LeftAudioFilePosition { get; set; } = 0;
        public float RightAudioFilePosition { get; set; } = 0;
        public SignalSource SignalSource { get; set; } = SignalSource.Oscillator;
        public float AudioFileGain { get; set; } = 100;
        public float AudioFilePitch { get; set; } = 1;
        public float AudioFileSpeed { get; set; } = 1;
        public bool IsLowPassEnabled { get; set; } = false;
        public float LowPassCutOffFrequence { get; set; } = 0.5f;
        public float LowPassResonance { get; set; } = 0.2f;
        public bool IsHighPassEnabled { get; set; } = false;
        public float HighPassCutOffFrequence { get; set; } = 0.5f;
        public float HighPassResonance { get; set; } = 0.2f;
        public float AttackTimeInMs { get; set; } = 50;
        public float DecayTimeInMs { get; set; } = 70;
        public float SustainVolume { get; set; } = 0.9f;
        public float ReleaseTimeInMs { get; set; } = 250;
        public bool UseDelayEffect { get; set; } = false;
        public bool UseHallEffect { get; set; } = false;
        public bool UseGainEffect { get; set; } = false;
        public float Gain { get; set; } = 7;
        public bool UsePitchEffect { get; set; } = false;
        public float PitchEffect { get; set; } = 1;
        public bool UseVolumeLfo { get; set; } = false;
        public float VolumeLfoFrequency { get; set; } = 5;
    }

    //Ein Synthesizer ist eine Menge von OscilatorWithLfo, dessen Frequenz und Amplitude über ein LFO gesteuert wird. Die per LFO gesteuerte Frequenz geht 
    //danach über die Frequenz-Rampe (Auch noch Bestandteil vom OscilatorWithLfo). Nach diesen so gesteuerten Ossilatoren vereint der Mixer die Oszis und 
    //danach kommt dann der Tiefpass-Filter und danach die Hüllkurve.
    //[OscilatorWithLfo] -> [MultiOscillator] -> [Filter] -> [AdsrEnvelope]
    public class Synthesizer : IPianoComponent
    {
        //Source 1: Oscilator
        private OscilatorWithLfo Oscilator;
        private MultiOscillator MultiOscillator;
        private Mixer OscWithSubOscMixer;

        //Source 2: AudioFile
        public AudioFile AudioFile { get; private set; } //Kann anstelle der gemixten Oszilatoren genommen werden

        //Source 3: Microfon
        public AudioRecorderPianoComponent AudioRecorder { get; private set; } //Kann anstelle der gemixten Oszilatoren genommen werden

        private Switch sourceSwitch;

        private Filter lowPass;
        private Filter highPass;
        private AdsrEnvelope adsrEnvelope;
        private DelayEffect delayEffect;
        private HallEffect hallEffect;
        private GainEffect gainEffect;
        private PitchEffect pitchEffect;
        private VolumeLfo VolumeLfo;

        private int sampleRate;
        private IPianoStopKeyHandler[] stopKeyHandler; //Sagen, wie lange nach dem Release-Key-Signal noch der Ton weiter geht 

        public Synthesizer(int sampleRate, IAudioRecorder audioRecorder)
        {
            this.sampleRate = sampleRate; //Wird für die Copy-Funktion benötigt

            //Source 1: Oscilator
            this.Oscilator = new OscilatorWithLfo(sampleRate);
            this.MultiOscillator = new MultiOscillator(this.Oscilator);
            this.OscWithSubOscMixer = new Mixer(this.MultiOscillator, new SubOscilator(sampleRate) { OsciType = SignalType.SawTooth }) { VolumeB = 0 };
            
            //Source 2: AudioFile
            this.AudioFile = new AudioFile(sampleRate);

            //Source 3: Microfon
            this.AudioRecorder = new AudioRecorderPianoComponent(audioRecorder);

            //Switch between Oscilator, AudioFile and Microfon
            this.sourceSwitch = new Switch(this.OscWithSubOscMixer, this.AudioFile, this.AudioRecorder);

            //Effects
            this.lowPass = new Filter(this.sourceSwitch, FilterType.LowPass, sampleRate) { CutOffFrequence = 0.5f };
            this.highPass = new Filter(this.lowPass, FilterType.HighPass, sampleRate) { CutOffFrequence = 0.5f };
            this.adsrEnvelope = new AdsrEnvelope(this.highPass, sampleRate);
            this.delayEffect = new DelayEffect(this.adsrEnvelope, sampleRate);
            this.hallEffect = new HallEffect(this.delayEffect, sampleRate);
            this.gainEffect = new GainEffect(this.hallEffect);
            this.pitchEffect = new PitchEffect(this.gainEffect, sampleRate);
            this.VolumeLfo = new VolumeLfo(this.pitchEffect, sampleRate);

            this.stopKeyHandler = new IPianoStopKeyHandler[]
            {
                this.adsrEnvelope,
                this.delayEffect,
                this.hallEffect
            };
        }

        public Synthesizer GetCopy()
        {
            var copy = new Synthesizer(this.sampleRate, this.AudioRecorder.AudioRecorder);

            var data = this.GetAllSettings();
            copy.SetAllSettings(data, null, "", this.sampleRate);
            copy.AudioFileData = this.AudioFileData;

            return copy;
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
            this.OsciType = data.OsciType;
            this.UseAccordEffect = data.UseAccordEffect;
            this.PusleWidth = data.PusleWidth;
            this.OsciCount = data.OsciCount;
            this.MultiOsciPitch = data.MultiOsciPitch;
            this.SubOszVolume = data.SubOszVolume;
            this.UseAmplitudeLfo = data.UseAmplitudeLfo;
            this.AmplitudeLfoFrequenc = data.AmplitudeLfoFrequenc;
            this.AmplitudeLfoAmplitude = data.AmplitudeLfoAmplitude;
            this.AmplitudeLfoPulseWidth = data.AmplitudeLfoPulseWidth;
            this.AudioFileName = data.AudioFileName;
            string absolutPath = searchDirectoryForAudioFiles + "\\" + data.AudioFileName;
            if (audioFileReader != null && File.Exists(absolutPath))
            {
                this.AudioFileData = audioFileReader.GetSamplesFromAudioFile(absolutPath, sampleRate);
            }
            this.LeftAudioFilePosition = data.LeftAudioFilePosition;
            this.RightAudioFilePosition = data.RightAudioFilePosition;  
            this.SignalSource = data.SignalSource;
            this.AudioFilePitch = data.AudioFilePitch;
            this.AudioFileSpeed = data.AudioFileSpeed;
            this.IsLowPassEnabled = data.IsLowPassEnabled;
            this.LowPassCutOffFrequence = data.LowPassCutOffFrequence;
            this.LowPassResonance = data.LowPassResonance;
            this.IsHighPassEnabled = data.IsHighPassEnabled;
            this.HighPassCutOffFrequence = data.HighPassCutOffFrequence;
            this.HighPassResonance = data.HighPassResonance;
            this.AttackTimeInMs = data.AttackTimeInMs;
            this.DecayTimeInMs = data.DecayTimeInMs;
            this.SustainVolume = data.SustainVolume;
            this.ReleaseTimeInMs = data.ReleaseTimeInMs;
            this.UseDelayEffect = data.UseDelayEffect;
            this.UseHallEffect = data.UseHallEffect;
            this.UseGainEffect = data.UseGainEffect;
            this.Gain = data.Gain;
            this.UsePitchEffect = data.UsePitchEffect;
            this.PitchEffect = data.PitchEffect;
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
                OsciType = this.OsciType,
                UseAccordEffect = this.UseAccordEffect,
                PusleWidth = this.PusleWidth,
                OsciCount = this.OsciCount,
                MultiOsciPitch = this.MultiOsciPitch,
                SubOszVolume = this.SubOszVolume,
                UseAmplitudeLfo = this.UseAmplitudeLfo,
                AmplitudeLfoFrequenc = this.AmplitudeLfoFrequenc,
                AmplitudeLfoAmplitude = this.AmplitudeLfoAmplitude,
                AmplitudeLfoPulseWidth = this.AmplitudeLfoPulseWidth,
                AudioFileName = this.AudioFileName,
                LeftAudioFilePosition = this.LeftAudioFilePosition,
                RightAudioFilePosition = this.RightAudioFilePosition,  
                SignalSource = this.SignalSource,
                AudioFilePitch = this.AudioFilePitch,
                AudioFileSpeed = this.AudioFileSpeed,
                IsLowPassEnabled = this.IsLowPassEnabled,
                LowPassCutOffFrequence = this.LowPassCutOffFrequence,
                LowPassResonance = this.LowPassResonance,
                IsHighPassEnabled = this.IsHighPassEnabled,
                HighPassCutOffFrequence = this.HighPassCutOffFrequence,
                HighPassResonance = this.HighPassResonance,
                AttackTimeInMs = this.AttackTimeInMs,
                DecayTimeInMs = this.DecayTimeInMs,
                SustainVolume = this.SustainVolume,
                ReleaseTimeInMs = this.ReleaseTimeInMs,
                UseDelayEffect = this.UseDelayEffect,
                UseHallEffect = this.UseHallEffect,
                UseGainEffect = this.UseGainEffect,
                Gain = this.Gain,
                UsePitchEffect = this.UsePitchEffect,
                PitchEffect = this.PitchEffect,
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


        public SignalType OsciType { get { return this.Oscilator.OsciType; } set { this.Oscilator.OsciType = value; } }
        public bool UseAccordEffect { get { return this.Oscilator.UseAccordEffect; } set { this.Oscilator.UseAccordEffect = value; } }
        public float PusleWidth { get { return this.Oscilator.PusleWidth; } set { this.Oscilator.PusleWidth = value; } }
        public int OsciCount { get { return this.MultiOscillator.OsciCount; } set { this.MultiOscillator.OsciCount = value; } }
        public float MultiOsciPitch { get { return this.MultiOscillator.Pitch; } set { this.MultiOscillator.Pitch = value; } }
        public float SubOszVolume { get { return this.OscWithSubOscMixer.VolumeB; } set { this.OscWithSubOscMixer.VolumeB = value; } }
        public bool UseAmplitudeLfo { get { return this.Oscilator.UseAmplitudeLfo; } set { this.Oscilator.UseAmplitudeLfo = value; } }
        public float AmplitudeLfoFrequenc { get { return this.Oscilator.AmplitudeLfoFrequenc; } set { this.Oscilator.AmplitudeLfoFrequenc = value; } }
        public float AmplitudeLfoAmplitude { get { return this.Oscilator.AmplitudeLfoAmplitude; } set { this.Oscilator.AmplitudeLfoAmplitude = value; } }
        public float AmplitudeLfoPulseWidth { get { return this.Oscilator.AmplitudeLfoPulseWidth; } set { this.Oscilator.AmplitudeLfoPulseWidth = value; } }
        

        public string AudioFileName { get { return this.AudioFile.AudioFileName; } set { this.AudioFile.AudioFileName = value; } }
        public float[] AudioFileData { get { return this.AudioFile.SampleData; } set { this.AudioFile.SampleData = value; } }
        public float LeftAudioFilePosition { get { return this.AudioFile.LeftPositionInMilliseconds; } set { this.AudioFile.LeftPositionInMilliseconds = value; } }
        public float RightAudioFilePosition { get { return this.AudioFile.RightPositionInMilliseconds; } set { this.AudioFile.RightPositionInMilliseconds = value; } }
        public SignalSource SignalSource { get => this.sourceSwitch.SignalSource; set => this.sourceSwitch.SignalSource = value; }
        public float AudioFilePitch { get { return this.AudioFile.Pitch; } set { this.AudioFile.Pitch = value; } }
        public float AudioFileSpeed { get { return this.AudioFile.Speed; } set { this.AudioFile.Speed = value; } }

        public bool IsLowPassEnabled { get { return this.lowPass.IsEnabled; } set { this.lowPass.IsEnabled = value; } }
        public float LowPassCutOffFrequence { get { return this.lowPass.CutOffFrequence; } set { this.lowPass.CutOffFrequence = value; } }
        public float LowPassResonance { get { return this.lowPass.Resonance; } set { this.lowPass.Resonance = value; } }
        public bool IsHighPassEnabled { get { return this.highPass.IsEnabled; } set { this.highPass.IsEnabled = value; } }
        public float HighPassCutOffFrequence { get { return this.highPass.CutOffFrequence; } set { this.highPass.CutOffFrequence = value; } }
        public float HighPassResonance { get { return this.highPass.Resonance; } set { this.highPass.Resonance = value; } }


        public float AttackTimeInMs { get { return this.adsrEnvelope.AttackTimeInMs; } set { this.adsrEnvelope.AttackTimeInMs = value; } }
        public float DecayTimeInMs { get { return this.adsrEnvelope.DecayTimeInMs; } set { this.adsrEnvelope.DecayTimeInMs = value; } }
        public float SustainVolume { get { return this.adsrEnvelope.SustainVolume; } set { this.adsrEnvelope.SustainVolume = value; } }
        public float ReleaseTimeInMs { get { return this.adsrEnvelope.ReleaseTimeInMs; } set { this.adsrEnvelope.ReleaseTimeInMs = value; } }
        

        public bool UseDelayEffect { get { return this.delayEffect.IsEnabled; } set { this.delayEffect.IsEnabled = value; } }
        public bool UseHallEffect { get { return this.hallEffect.IsEnabled; } set { this.hallEffect.IsEnabled = value; } }
        public bool UseGainEffect { get { return this.gainEffect.IsEnabled; } set { this.gainEffect.IsEnabled = value; } }
        public float Gain { get { return this.gainEffect.Gain; } set { this.gainEffect.Gain = value; } }
        public bool UsePitchEffect { get { return this.pitchEffect.IsEnabled; } set { this.pitchEffect.IsEnabled = value; } }
        public float PitchEffect { get { return this.pitchEffect.Pitch; } set { this.pitchEffect.Pitch = value; } }
        public bool UseVolumeLfo { get { return this.VolumeLfo.IsEnabled; } set { this.VolumeLfo.IsEnabled = value; } }
        public float VolumeLfoFrequency { get { return this.VolumeLfo.Frequency; } set { this.VolumeLfo.Frequency = value; } }
    }
}


