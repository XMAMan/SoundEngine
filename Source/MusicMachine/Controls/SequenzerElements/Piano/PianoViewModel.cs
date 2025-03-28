using ReactiveUI;
using System.Reactive;
using WaveMaker.Sequenzer;

namespace MusicMachine.Controls.SequenzerElements.Piano
{
    public class PianoViewModel : ReactiveObject
    {
        private PianoSequenzer model;

        private int[] pressedKeys = Enumerable.Repeat(-1, 256).ToArray();

        public PianoViewModel(PianoSequenzer sequenzer)
        {
            this.model = sequenzer;


            //Spiele Tone vom PianoCanvas
            this.PlayToneCommand = ReactiveCommand.Create<int, Unit>((toneIndex) =>
            {
                if (this.pressedKeys[toneIndex] == -1)
                {
                    this.pressedKeys[toneIndex] = this.model.StartPlayingKey(toneIndex);
                }
                
                return Unit.Default;
            });
            //Lasse Taste los vom PianoCanvas
            this.ReleaseToneCommand = ReactiveCommand.Create<int, Unit>((toneIndex) =>
            {
                if (this.pressedKeys[toneIndex] != -1)
                {
                    this.model.ReleaseKey(this.pressedKeys[toneIndex]);
                    this.pressedKeys[toneIndex] = -1;
                }
                
                return Unit.Default;
            });
        }


        public ReactiveCommand<int, Unit> PlayToneCommand { get; private set; } //Bei Druck auf PianoControl
        public ReactiveCommand<int, Unit> ReleaseToneCommand { get; private set; } //Bei Loslassen auf PianoControl
    }
}
