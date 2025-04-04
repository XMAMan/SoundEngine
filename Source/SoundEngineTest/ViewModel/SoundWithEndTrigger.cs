using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using SoundEngine.SoundSnippeds;
using System.Reactive;

namespace SoundEngineTest.ViewModel
{
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
}
