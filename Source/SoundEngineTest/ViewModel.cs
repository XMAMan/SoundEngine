using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SoundEngine;
using SoundEngine.SoundSnippeds;
using System;
using System.Reactive;
using WaveMaker.KeyboardComponents;

namespace SoundEngineTest
{
    public class ViewModel : ReactiveObject, IDisposable
    {
        private SoundTable table = new SoundTable();
        public float Volume { get { return this.table.Volume; } set { this.table.Volume = value; } }
        public MusicFileSnippedViewModel BackGroundMusic { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts0 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts1 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts2 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts3 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts4 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts5 { get; private set; }
        public FrequenceToneSnippedViewModel TieferBass { get; private set; }
        public AudioFileSnippedViewModel Soundsystem { get; private set; }
        public FrequenceToneSnippedViewModel Hallo { get; private set; }
        public ViewModel()
        {
            this.SoundEffekts0 = new FrequenceToneSnippedViewModel(table.SoundEffekts0);
            this.SoundEffekts1 = new FrequenceToneSnippedViewModel(table.SoundEffekts1);
            this.SoundEffekts2 = new FrequenceToneSnippedViewModel(table.SoundEffekts2);
            this.SoundEffekts3 = new FrequenceToneSnippedViewModel(table.SoundEffekts3);
            this.SoundEffekts4 = new FrequenceToneSnippedViewModel(table.SoundEffekts4);
            this.SoundEffekts5 = new FrequenceToneSnippedViewModel(table.SoundEffekts5);

            this.TieferBass = new FrequenceToneSnippedViewModel(table.TieferBass);
            this.BackGroundMusic = new MusicFileSnippedViewModel(table.BackGroundMusic);
            this.Soundsystem = new AudioFileSnippedViewModel(table.Soundsystem);
            this.Hallo = new FrequenceToneSnippedViewModel(table.Hallo);
        }
        public void Dispose()
        {
            this.table.Dispose();
        }
    }

    public class SoundWithEndTrigger : ReactiveObject
    {
        private ISoundSnippedWithEndTrigger snipp;
        public SoundWithEndTrigger(ISoundSnippedWithEndTrigger snipp)
        {
            this.snipp = snipp;
            this.snipp.IsRunningChanged = (isRunning) => { this.IsRunning = isRunning; };

            this.Play = ReactiveCommand.Create(() =>
            {
                this.snipp.Play();
            });
            this.Stop = ReactiveCommand.Create(() =>
            {
                this.snipp.Stop();
            });
            this.Reset = ReactiveCommand.Create(() =>
            {
                this.snipp.Reset();
            });

            this.snipp.EndTrigger = () => { this.EndTriggerTime = DateTime.Now.ToString(); };
        }
        [Reactive] public bool IsRunning { get; private set; } = false;
        public ReactiveCommand<Unit, Unit> Play { get; private set; }
        public ReactiveCommand<Unit, Unit> Stop { get; private set; }
        public float Volume { get { return this.snipp.Volume; } set { this.snipp.Volume = value; } }
        public ReactiveCommand<Unit, Unit> Reset { get; private set; }
        public bool AutoLoop { get { return this.snipp.AutoLoop; } set { this.snipp.AutoLoop = value; } }
        [Reactive] public string EndTriggerTime { get; set; } = "";
    }

    public class MusicFileSnippedViewModel : SoundWithEndTrigger
    {
        private IMusicFileSnipped snipp;
        public MusicFileSnippedViewModel(IMusicFileSnipped snip)
            :base(snip)
        {
            this.snipp = snip;
        }
        public float KeyStrokeSpeed { get { return this.snipp.KeyStrokeSpeed; } set { this.snipp.KeyStrokeSpeed = value; } }

    }

    public class AudioFileSnippedViewModel : SoundWithEndTrigger
    {
        private IAudioFileSnipped snipp;
        
        public AudioFileSnippedViewModel(IAudioFileSnipped snipp)
            :base(snipp)
        {
            this.snipp = snipp;
        }

        public float Pitch { get { return this.snipp.Pitch; } set { this.snipp.Pitch = value; } }
        public float Speed { get { return this.snipp.Speed; } set { this.snipp.Speed = value; } }
        public bool UseDelayEffekt { get { return this.snipp.UseDelayEffekt; } set { this.snipp.UseDelayEffekt = value; } }
        public bool UseHallEffekt { get { return this.snipp.UseHallEffekt; } set { this.snipp.UseHallEffekt = value; } }
        public bool UseGainEffekt { get { return this.snipp.UseGainEffekt; } set { this.snipp.UseGainEffekt = value; } }
        public float Gain { get { return this.snipp.Gain; } set { this.snipp.Gain = value; } }
        public bool UseVolumeLfo { get { return this.snipp.UseVolumeLfo; } set { this.snipp.UseVolumeLfo = value; } }
        public float VolumeLfoFrequency { get { return this.snipp.VolumeLfoFrequency; } set { this.snipp.VolumeLfoFrequency = value; } }

    }

    public class FrequenceToneSnippedViewModel : ReactiveObject
    {
        protected IFrequenceToneSnipped snipp;
        public FrequenceToneSnippedViewModel(IFrequenceToneSnipped snipp)
        {
            this.snipp = snipp;
            this.snipp.IsRunningChanged = (isRunning) => { this.IsRunning = isRunning; };

            this.Play = ReactiveCommand.Create(() =>
            {
                this.snipp.Play();
            });
            this.Stop = ReactiveCommand.Create(() =>
            {
                this.snipp.Stop();
            });

        }
        [Reactive] public bool IsRunning { get; private set; } = false;
        public ReactiveCommand<Unit, Unit> Play { get; private set; }
        public ReactiveCommand<Unit, Unit> Stop { get; private set; }
        public float Volume { get { return this.snipp.Volume; } set { this.snipp.Volume = value; } }
        public float Frequency { get { return this.snipp.Frequency; } set { this.snipp.Frequency = value; } }
        public Synthesizer Synthesizer { get => this.snipp.Synthesizer; }
    }

    public class SoundTable : IDisposable
    {
        private string WorkingDirectory = @"..\..\..\..\Data\";

        private SoundGenerator soundGenerator;

        public float Volume { get { return this.soundGenerator.Volume; } set { this.soundGenerator.Volume = value; } }
        public IMusicFileSnipped BackGroundMusic { get; private set; }
        public IFrequenceToneSnipped SoundEffekts0 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts1 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts2 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts3 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts4 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts5 { get; private set; }
        public IFrequenceToneSnipped TieferBass { get; private set; }
        public IAudioFileSnipped Soundsystem { get; private set; }
        public IFrequenceToneSnipped Hallo { get; private set; }
        public SoundTable()
        {
            this.soundGenerator = new SoundGenerator();

            var syntSounds = this.soundGenerator.AddSynthSoundCollection(WorkingDirectory + "SoundEffekts.music");

            this.SoundEffekts0 = syntSounds[0];
            this.SoundEffekts1 = syntSounds[1];
            this.SoundEffekts2 = syntSounds[2];
            this.SoundEffekts3 = syntSounds[3];
            this.SoundEffekts4 = syntSounds[4];
            this.SoundEffekts5 = syntSounds[5];
            this.TieferBass = this.soundGenerator.AddFrequencyTone(WorkingDirectory + "TieferBass.synt");

            this.BackGroundMusic = this.soundGenerator.AddMusicFile(WorkingDirectory + "lied3.music");
            this.Soundsystem = this.soundGenerator.AddSoundFile(WorkingDirectory + "Soundsystem.mp3");
            this.Hallo = this.soundGenerator.AddFrequencyTone(WorkingDirectory + "Hallo.synt");
        }

        public void Dispose()
        {
            this.soundGenerator.Dispose();
        }
    }
}
