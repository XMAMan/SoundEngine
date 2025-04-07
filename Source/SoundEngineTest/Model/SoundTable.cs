using SoundEngine.SoundSnippeds;
using SoundEngine;

namespace SoundEngineTest.Model
{
    public class SoundTable : IDisposable
    {
        private string WorkingDirectory = @"..\..\..\..\..\Data\";

        private SoundGenerator soundGenerator;

        public float Volume { get { return this.soundGenerator.Volume; } set { this.soundGenerator.Volume = value; } }
        public string SelectedOutputDevice { get { return this.soundGenerator.SelectedOutputDevice; } set { this.soundGenerator.SelectedOutputDevice = value; } }
        public string[] GetAvailableOutputDevices() { return this.soundGenerator.GetAvailableOutputDevices(); }

        public IMusicFileSnipped BackGroundMusic { get; private set; }
        public IMusicFileSnipped MarioStart { get; private set; }
        public IFrequenceToneSnipped SoundEffects0 { get; private set; }
        public IFrequenceToneSnipped SoundEffects1 { get; private set; }
        public IFrequenceToneSnipped SoundEffects2 { get; private set; }
        public IFrequenceToneSnipped SoundEffects3 { get; private set; }
        public IFrequenceToneSnipped SoundEffects4 { get; private set; }
        public IFrequenceToneSnipped SoundEffects5 { get; private set; }
        public IFrequenceToneSnipped SoundEffects6 { get; private set; }
        public IFrequenceToneSnipped SoundEffects7 { get; private set; }
        public IFrequenceToneSnipped TieferBass { get; private set; }
        public IAudioFileSnipped Soundsystem { get; private set; }
        public IAudioFileSnipped GlassBroke { get; private set; }
        public IFrequenceToneSnipped Hallo { get; private set; }
        public IAudioRecorderSnipped AudioRecorder { get; private set; }
        public SoundTable()
        {
            this.soundGenerator = new SoundGenerator();

            var syntSounds = this.soundGenerator.AddSynthSoundCollection(WorkingDirectory + "SoundEffects.music");

            this.SoundEffects0 = syntSounds[0];
            this.SoundEffects1 = syntSounds[1];
            this.SoundEffects2 = syntSounds[2];
            this.SoundEffects3 = syntSounds[3];
            this.SoundEffects4 = syntSounds[4];
            this.SoundEffects5 = syntSounds[5];
            this.SoundEffects6 = syntSounds[6];
            this.SoundEffects7 = syntSounds[7];
            this.TieferBass = this.soundGenerator.AddFrequencyTone(WorkingDirectory + "TieferBass.synt");

            this.BackGroundMusic = this.soundGenerator.AddMusicFile(WorkingDirectory + "lied3.music");
            this.MarioStart = this.soundGenerator.AddMusicFile(WorkingDirectory + "MarioStart.music");
            this.Soundsystem = this.soundGenerator.AddSoundFile(WorkingDirectory + "Soundsystem.mp3");
            this.GlassBroke = this.soundGenerator.AddSoundFile(WorkingDirectory + "GlassBroke.wav");
            this.Hallo = this.soundGenerator.AddFrequencyTone(WorkingDirectory + "Hallo.synt");

            this.AudioRecorder = this.soundGenerator.AudioRecorder;
        }

        public void Dispose()
        {
            this.soundGenerator.Dispose();
        }
    }
}
