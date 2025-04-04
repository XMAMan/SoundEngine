using SoundEngine.SoundSnippeds;

namespace SoundEngineTest.Model
{
    //Erzeugt ein Ton über ein ISoundSnipped-Objekt
    //Sollte dieses Interface noch zusätzlich das ISoundSnippedWithEndTrigger implementieren, dann bestimmt das Ende des Tones die Lebensdauer
    //Sollte der Ton potentiell unendlich spielbar sein, so wird der Ton nach n Sekunden gestoppt
    internal class Ball
    {
        public ISoundSnipped SoundSnipped { get; private set; }
        private int maxToneLength;
        private DateTime startTime = DateTime.Now;
        public Ball(ISoundSnipped soundSnipped, int maxToneLength = 999)
        {
            this.SoundSnipped = soundSnipped;
            this.maxToneLength = maxToneLength;

            if (soundSnipped is ISoundSnippedWithEndTrigger)
            {
                var soundSnippedWithEndTrigger = (ISoundSnippedWithEndTrigger)soundSnipped;
                soundSnippedWithEndTrigger.EndTrigger = () =>
                {
                    Stop();
                };
            }

            this.SoundSnipped.Play();
        }

        public event Action<Ball> EndTrigger = null; //Wird aufgerufen, wenn der Ton zu Ende ist

        public void CheckIfMaxToneLengthIsReached()
        {
            if (DateTime.Now - this.startTime > TimeSpan.FromSeconds(this.maxToneLength))
            {
                Stop();
            }
        }

        public void Stop()
        {
            this.SoundSnipped.Stop();
            this.EndTrigger?.Invoke(this);
            this.SoundSnipped.Dispose();
        }
    }
}
