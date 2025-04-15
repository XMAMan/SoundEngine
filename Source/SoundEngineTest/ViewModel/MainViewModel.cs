using ReactiveUI;
using SoundEngineTest.Controls.AudioFile;
using SoundEngineTest.Controls.AudioRecorder;
using SoundEngineTest.Controls.FreqeunceTone;
using SoundEngineTest.Controls.MusicFile;
using SoundEngineTest.Model;
using System.Reactive.Linq;

namespace SoundEngineTest.ViewModel
{
    public class MainViewModel : ReactiveObject, IDisposable
    {
        private SoundTable table = new SoundTable();

        private IDisposable timer;
        private List<ITimerTickHandler> timerTickHandlers = new List<ITimerTickHandler>();

        public float Volume { get { return this.table.Volume; } set { this.table.Volume = value; } }

        //Single-Sound-Testing
        public MusicFileSnippedViewModel BackGroundMusic { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffects0 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffects1 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffects2 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffects3 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffects4 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffects5 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffects6 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffects7 { get; private set; }
        public FrequenceToneSnippedViewModel TieferBass { get; private set; }
        public AudioFileSnippedViewModel Soundsystem { get; private set; }
        public FrequenceToneSnippedViewModel Hallo { get; private set; }
        public AudioRecorderSnippedViewModel AudioRecorder { get; private set; }

        //Multi-Sound-Testing
        public MultiFrequencyToneViewModel SoundEffects0Multi { get; private set; }
        public MultiMusicFileViewModel MultiMusicFileViewModel { get; private set; }
        public MultiAudioFileViewModel MultiAudioFile { get; private set; }

        public MainViewModel()
        {
            this.SoundEffects0 = new FrequenceToneSnippedViewModel(table.SoundEffects0);
            this.SoundEffects1 = new FrequenceToneSnippedViewModel(table.SoundEffects1);
            this.SoundEffects2 = new FrequenceToneSnippedViewModel(table.SoundEffects2);
            this.SoundEffects3 = new FrequenceToneSnippedViewModel(table.SoundEffects3);
            this.SoundEffects4 = new FrequenceToneSnippedViewModel(table.SoundEffects4);
            this.SoundEffects5 = new FrequenceToneSnippedViewModel(table.SoundEffects5);
            this.SoundEffects6 = new FrequenceToneSnippedViewModel(table.SoundEffects6);
            this.SoundEffects7 = new FrequenceToneSnippedViewModel(table.SoundEffects7);

            this.TieferBass = new FrequenceToneSnippedViewModel(table.TieferBass);
            this.BackGroundMusic = new MusicFileSnippedViewModel(table.BackGroundMusic);
            this.Soundsystem = new AudioFileSnippedViewModel(table.Soundsystem);
            this.Hallo = new FrequenceToneSnippedViewModel(table.Hallo);

            this.AudioRecorder = new AudioRecorderSnippedViewModel(table.SoundGenerator);


            this.SoundEffects0Multi = new MultiFrequencyToneViewModel(table.SoundEffects0);
            this.timerTickHandlers.Add(this.SoundEffects0Multi);

            this.MultiMusicFileViewModel = new MultiMusicFileViewModel(table.MarioStart);
            this.MultiAudioFile = new MultiAudioFileViewModel(table.GlassBroke);

            this.timer = Observable.Interval(TimeSpan.FromMilliseconds(250))
                .Subscribe(x =>
                {
                    foreach (var handler in this.timerTickHandlers)
                    {
                        handler.HandleTimerTick();
                    }
                });
        }

        public IEnumerable<string> Outputdevice
        {
            get
            {
                return this.table.GetAvailableOutputDevices();
            }
        }

        public string SelectedOutputdevice
        {
            get { return this.table.SelectedOutputDevice; }
            set
            {
                this.table.SelectedOutputDevice = value;
                this.RaisePropertyChanged(nameof(SelectedOutputdevice));
            }
        }

        public void Dispose()
        {
            this.table.Dispose();
            this.timer?.Dispose();
        }


    }
}
