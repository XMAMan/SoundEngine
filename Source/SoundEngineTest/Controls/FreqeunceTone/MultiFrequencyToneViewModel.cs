using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SoundEngine.SoundSnippeds;
using SoundEngineTest.Model;
using SoundEngineTest.ViewModel;
using System.Reactive;

namespace SoundEngineTest.Controls.FreqeunceTone
{
    public class MultiFrequencyToneViewModel : ReactiveObject, ITimerTickHandler
    {
        protected IFrequenceToneSnipped snipp;
        private List<Ball> balls = new List<Ball>();
        public MultiFrequencyToneViewModel(IFrequenceToneSnipped snipp)
        {
            this.snipp = snipp;

            Play = ReactiveCommand.Create(() =>
            {
                var snipped = this.snipp.GetCopy();

                var ball = new Ball(snipped, this.ToneLength);
                ball.EndTrigger += (b) =>
                {                    
                    this.balls.Remove(b);
                    this.ActiveCounter = this.balls.Count;
                };

                this.balls.Add(ball);
                this.ActiveCounter = this.balls.Count;
            });
            Stop = ReactiveCommand.Create(() =>
            {
                foreach (var ball in this.balls.ToList())
                {
                    ball.Stop();
                }
            });
        }

        public ReactiveCommand<Unit, Unit> Play { get; private set; }
        public ReactiveCommand<Unit, Unit> Stop { get; private set; }
        [Reactive] public int ToneLength { get; set; } = 3;
        [Reactive] public int ActiveCounter { get; set; } = 0;
        public float Frequency { get { return snipp.Frequency; } set { snipp.Frequency = value; } }

        public void HandleTimerTick()
        {
            foreach (var ball in this.balls.ToList())
            {
                ball.CheckIfMaxToneLengthIsReached();
            }
        }
    }

}
