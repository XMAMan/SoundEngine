namespace SignalAnalyser
{
    //Ein Signal sind die Abtastwerte einer einzelnen Periode von einer periodischen Funktion. (Dargetsellt als float-Array)
    //Ein FrequenzeSpaceSignal gibt auf der X-Achse die Frequenz und auf der Y-Achse die Amplitude eines Signals an, was in seine Sinus/Cosinus-Funktionen zerlegt wurde
    public class FrequenzeSpaceSignal
    {
        private ComplexNumber[] numbers; //numbers.Length = Mit so vielen Abtastwerten wurde ein TimeSpace-Signal abgetastet

        public int MaxFrequence { get; private set; }

        internal FrequenzeSpaceSignal(ComplexNumber[] numbers)
        {
            this.numbers = numbers;
            this.MaxFrequence = numbers.Length / 2;
        }

        public float GetAmplitude(int frequence) //frequence geht von 0 bis signal.Length / 2
        {
            return this.numbers[frequence].Y * -2;
        }

        public void SetAmplitude(int frequence, float amplitude)
        {
            this.numbers[frequence].Y = amplitude / -2;
            this.numbers[this.numbers.Length - frequence].Y = amplitude / 2;
        }

        public static FrequenzeSpaceSignal CreateFromTimeSpace(float[] signal)
        {
            //Geht beides
            return new FrequenzeSpaceSignal(DiskreteFouriertransformation.TransformFromTimeToFrequenceSpace(signal));
            //return new FrequenzeSpaceSignal(FastDiskreteFourierTransform.TransformFromTimeToFrequenceSpace(signal));
        }

        public float[] ToTimeSpace()
        {
            return DiskreteFouriertransformation.TransformFromFrequnceToTimeSpace(this.numbers);
            //return FastDiskreteFourierTransform.TransformFromFrequnceToTimeSpace(this.numbers);
        }
    }
}
