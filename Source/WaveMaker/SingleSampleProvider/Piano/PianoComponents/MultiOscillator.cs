namespace WaveMaker.KeyboardComponents
{
    //Mehrere Oszis mit ganz kleiner versetzer Frequenz
    public class MultiOscillator : IPianoComponent
    {
        private Oscillator oscillator;
        public int OsziCount { get; set; } = 2;
        public float Pitch { get; set; } = 3;
        public MultiOscillator(Oscillator oscillator)
        {
            this.oscillator = oscillator;
        }

        public float GetSample(KeySampleData data)
        {
            float sum = 0;
            for (int i=0;i<this.OsziCount;i++)
            {
                float frequency = data.Frequency + i * this.Pitch;

                sum += this.oscillator.GetSampleFromFrequence(data.SampleIndex, frequency, this.oscillator.PusleWidth) * (1.0f / this.OsziCount);
            }

            return sum;            
        }
    }
}
