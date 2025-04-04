using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SoundEngine.SoundSnippeds;
using SoundEngineTest.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace SoundEngineTest.Controls.AudioFile
{
    public class MultiAudioFile : ReactiveObject
    {
        protected IAudioFileSnipped snipp;
        private List<Ball> balls = new List<Ball>();
        public MultiAudioFile(IAudioFileSnipped snipp)
        {
            this.snipp = snipp;

            Play = ReactiveCommand.Create(() =>
            {
                var snipped = this.snipp.GetCopy();

                var ball = new Ball(snipped);
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
        [Reactive] public int ActiveCounter { get; set; } = 0;
    }
}
