using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SoundEngine;
using SoundEngine.SoundSnippeds;
using System;
using System.Reactive;

namespace SoundEngineTest
{
    public class ViewModel : ReactiveObject, IDisposable
    {
        private SoundTable table = new SoundTable();
        public float Volume { get { return this.table.Volume; } set { this.table.Volume = value; } }
        public AudioFileSnippedViewModel BackGroundMusic { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts0 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts1 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts2 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts3 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts4 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts5 { get; private set; }
        public FrequenceToneSnippedViewModel TieferBass { get; private set; }
        public AudioFileSnippedViewModel DasIstEinTest { get; private set; }
        public FrequenceToneFromAudioFileSnippedViewModel Hallo { get; private set; }
        public ViewModel()
        {
            this.SoundEffekts0 = new FrequenceToneSnippedViewModel(table.SoundEffekts0);
            this.SoundEffekts1 = new FrequenceToneSnippedViewModel(table.SoundEffekts1);
            this.SoundEffekts2 = new FrequenceToneSnippedViewModel(table.SoundEffekts2);
            this.SoundEffekts3 = new FrequenceToneSnippedViewModel(table.SoundEffekts3);
            this.SoundEffekts4 = new FrequenceToneSnippedViewModel(table.SoundEffekts4);
            this.SoundEffekts5 = new FrequenceToneSnippedViewModel(table.SoundEffekts5);

            this.TieferBass = new FrequenceToneSnippedViewModel(table.TieferBass);
            this.BackGroundMusic = new AudioFileSnippedViewModel(table.BackGroundMusic);
            this.DasIstEinTest = new AudioFileSnippedViewModel(table.DasIstEinTest);
            this.Hallo = new FrequenceToneFromAudioFileSnippedViewModel(table.Hallo);
        }
        public void Dispose()
        {
            this.table.Dispose();
        }
    }

    public class AudioFileSnippedViewModel : ReactiveObject
    {
        private IAudioFileSnipped snipp;
        public AudioFileSnippedViewModel(IAudioFileSnipped snipp)
        {
            this.snipp = snipp;
            this.snipp.IsRunningChanged = (isRunning)=>{ this.IsRunning = isRunning;  };

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
        }
        [Reactive] public bool IsRunning { get; private set; } = false;
        public ReactiveCommand<Unit, Unit> Play { get; private set; }
        public ReactiveCommand<Unit, Unit> Stop { get; private set; }
        public float Volume { get { return this.snipp.Volume; } set { this.snipp.Volume = value; } }
        public ReactiveCommand<Unit, Unit> Reset { get; private set; }
        public bool AutoLoop { get { return this.snipp.AutoLoop; } set { this.snipp.AutoLoop = value; } }
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
        
    }

    public class FrequenceToneFromAudioFileSnippedViewModel : FrequenceToneSnippedViewModel
    {
        public FrequenceToneFromAudioFileSnippedViewModel(IFrequenceToneSnipped snipp)
            :base(snipp)
        { }

        public float Pitch { get { return this.snipp.Pitch; } set { this.snipp.Pitch = value; } }
    }

    public class SoundTable : IDisposable
    {
        private string WorkingDirectory = @"..\..\..\..\Data\";

        private SoundGenerator soundGenerator;

        public float Volume { get { return this.soundGenerator.Volume; } set { this.soundGenerator.Volume = value; } }
        public IAudioFileSnipped BackGroundMusic { get; private set; }
        public IFrequenceToneSnipped SoundEffekts0 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts1 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts2 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts3 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts4 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts5 { get; private set; }
        public IFrequenceToneSnipped TieferBass { get; private set; }
        public IAudioFileSnipped DasIstEinTest { get; private set; }
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
            this.DasIstEinTest = this.soundGenerator.AddSoundFile(WorkingDirectory + "Soundsystem.mp3");
            this.Hallo = this.soundGenerator.AddFrequencyTone(WorkingDirectory + "Hallo.synt");
        }

        public void Dispose()
        {
            this.soundGenerator.Dispose();
        }
    }
}
