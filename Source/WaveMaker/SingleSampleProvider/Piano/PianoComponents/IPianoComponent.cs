namespace WaveMaker.KeyboardComponents
{
    //Wird mit Drücken einer Keyboard-Taste erzeugt und bleibt so lange, bis Taste losgelassen wurde und Hüllkurve KeyIsFinished gesetzt hat
    public class KeySampleData
    {
        public int SampleIndex = 0; //Geht von 0 (Taste wurde gedrückt) bis KeyUpSampleIndex+MaxStopTime
        public float Frequency { get; set; } //Gibt das Keyboard vor; Wird vom Oszilator gelesen
        public int KeyUpSampleIndex = int.MaxValue; //Wird vom KeyUp-Handler des Keyboards gesetzt (Die Hüllkurve braucht das, damit sie weiß, ab wann die Release-Phase beginnt)

        public KeySampleData(float frequency)
        {
            this.Frequency = frequency;
        }
    }

    //Im Gegensatz zum ISingleSampleProvider wird nicht einfach nur für ein gegebenen SampleIndex ein Samplewert ermittelt sondern die Information, welche Frequenz
    //gespielt werden soll, und bei welchen SampleIndex die Taste losgelassen wurde, ist hier auch noch wichtig
    public interface IPianoComponent
    {
        float GetSample(KeySampleData data);
    }

    //Reagiert diese Komponente auf das KeyUp-Signal?  Beispiele: AdsrEnvelope, DelayEffect
    public interface IPianoStopKeyHandler : IPianoComponent
    {
        bool IsEnabled { get; }     //Ist der Effekt eingeschaltet?
        int StopIndexTime { get; }  //So viele Samples nach dem KeyUp-Signal kann der Ton dann gelöscht werden
    }
}
