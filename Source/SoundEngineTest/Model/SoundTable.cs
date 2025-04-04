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
        public IFrequenceToneSnipped SoundEffekts0 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts1 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts2 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts3 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts4 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts5 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts6 { get; private set; }
        public IFrequenceToneSnipped SoundEffekts7 { get; private set; }
        public IFrequenceToneSnipped TieferBass { get; private set; }
        public IAudioFileSnipped Soundsystem { get; private set; }
        public IAudioFileSnipped GlassBroke { get; private set; }
        public IFrequenceToneSnipped Hallo { get; private set; }
        public IAudioRecorderSnipped AudioRecorder { get; private set; }
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
            this.SoundEffekts6 = syntSounds[6];
            this.SoundEffekts7 = syntSounds[7];
            this.TieferBass = this.soundGenerator.AddFrequencyTone(WorkingDirectory + "TieferBass.synt");

            this.BackGroundMusic = this.soundGenerator.AddMusicFile(WorkingDirectory + "lied3.music");
            this.MarioStart = this.soundGenerator.AddMusicFile(WorkingDirectory + "MarioStart.music");
            this.Soundsystem = this.soundGenerator.AddSoundFile(WorkingDirectory + "Soundsystem.mp3");
            this.GlassBroke = this.soundGenerator.AddSoundFile(WorkingDirectory + "GlassBroke.wav");
            this.Hallo = this.soundGenerator.AddFrequencyTone(WorkingDirectory + "Hallo.synt");

            this.AudioRecorder = this.soundGenerator.AudioRecorderSnipped;
        }

        public void Dispose()
        {
            this.soundGenerator.Dispose();
        }
    }
}
