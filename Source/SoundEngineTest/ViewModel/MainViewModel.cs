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
        public FrequenceToneSnippedViewModel SoundEffekts0 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts1 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts2 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts3 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts4 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts5 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts6 { get; private set; }
        public FrequenceToneSnippedViewModel SoundEffekts7 { get; private set; }
        public FrequenceToneSnippedViewModel TieferBass { get; private set; }
        public AudioFileSnippedViewModel Soundsystem { get; private set; }
        public FrequenceToneSnippedViewModel Hallo { get; private set; }
        public AudioRecorderSnippedViewModel AudioRecorder { get; private set; }

        //Multi-Sound-Testing
        public MultiFrequencyToneViewModel SoundEffekts0Multi { get; private set; }
        public MultiMusicFileViewModel MultiMusicFileViewModel { get; private set; }
        public MultiAudioFile MultiAudioFile { get; private set; }

        public MainViewModel()
        {
            this.SoundEffekts0 = new FrequenceToneSnippedViewModel(table.SoundEffekts0);
            this.SoundEffekts1 = new FrequenceToneSnippedViewModel(table.SoundEffekts1);
            this.SoundEffekts2 = new FrequenceToneSnippedViewModel(table.SoundEffekts2);
            this.SoundEffekts3 = new FrequenceToneSnippedViewModel(table.SoundEffekts3);
            this.SoundEffekts4 = new FrequenceToneSnippedViewModel(table.SoundEffekts4);
            this.SoundEffekts5 = new FrequenceToneSnippedViewModel(table.SoundEffekts5);
            this.SoundEffekts6 = new FrequenceToneSnippedViewModel(table.SoundEffekts6);
            this.SoundEffekts7 = new FrequenceToneSnippedViewModel(table.SoundEffekts7);

            this.TieferBass = new FrequenceToneSnippedViewModel(table.TieferBass);
            this.BackGroundMusic = new MusicFileSnippedViewModel(table.BackGroundMusic);
            this.Soundsystem = new AudioFileSnippedViewModel(table.Soundsystem);
            this.Hallo = new FrequenceToneSnippedViewModel(table.Hallo);

            this.AudioRecorder = new AudioRecorderSnippedViewModel(table.AudioRecorder);


            this.SoundEffekts0Multi = new MultiFrequencyToneViewModel(table.SoundEffekts0);
            this.timerTickHandlers.Add(this.SoundEffekts0Multi);

            this.MultiMusicFileViewModel = new MultiMusicFileViewModel(table.MarioStart);
            this.MultiAudioFile = new MultiAudioFile(table.GlassBroke);

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
