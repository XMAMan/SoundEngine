using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using SoundEngine.SoundSnippeds;
using System.Reactive;
using WaveMaker.KeyboardComponents;

namespace SoundEngineTest.Controls.FreqeunceTone
{
    public class FrequenceToneSnippedViewModel : ReactiveObject
    {
        protected IFrequenceToneSnipped snipp;
        public FrequenceToneSnippedViewModel(IFrequenceToneSnipped snipp)
        {
            this.snipp = snipp;
            this.snipp.IsRunningChanged = (isRunning) => { IsRunning = isRunning; };

            Play = ReactiveCommand.Create(() =>
            {
                this.snipp.Play();
            });
            Stop = ReactiveCommand.Create(() =>
            {
                this.snipp.Stop();
            });

        }
        [Reactive] public bool IsRunning { get; private set; } = false;
        public ReactiveCommand<Unit, Unit> Play { get; private set; }
        public ReactiveCommand<Unit, Unit> Stop { get; private set; }
        public float Volume { get { return snipp.Volume; } set { snipp.Volume = value; } }
        public float Frequency { get { return snipp.Frequency; } set { snipp.Frequency = value; } }
        public Synthesizer Synthesizer { get => snipp.Synthesizer; }
    }
}
