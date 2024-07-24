using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalAnalyser
{
    //Wandelt von ein Signal mit beliebiger Sampleanzahl, wo das Signal genau eine Periode angibt, zwischen Zeitraum und Frequenzraum um
    //https://www.tu-chemnitz.de/informatik/ThIS/downloads/courses/ws02/datkom/Fouriertransformation.pdf
    static class DiskreteFouriertransformation
    {
        //signal = Enthält die Abtastwerte von ein periodischen Signal. Alle Einträge beschreiben genau eine Periode.
        //         D.h. das Signal ist von der zeitlichen Breite so breit, wie die Minimalfrequenz, die ich abtasten will und 
        //         enthält doppelt so viele Samples wie die Maximalfrequenz, die ich abtasten will
        //         Habe ich ein Tonsignal mit S=22000 Samples pro Sekunde, und ich gebe von einer Sekunde die S Samples rein, dann geht die Frequenz von 1Hz bis 11kHz
        //         Das Array darf eine beliebiger Länge haben. Es sollte aber doppelt so viele Abtastwerte haben, wie die
        //         höchste Frequenz, die im Signal enthalten ist.
        internal static ComplexNumber[] TransformFromTimeToFrequenceSpace(float[] signal)
        {
            ComplexNumber[] input = signal.Select(x => new ComplexNumber(x, 0)).ToArray();
            return Transform(true, input);
        }

        internal static float[] TransformFromFrequnceToTimeSpace(ComplexNumber[] signal)
        {
            ComplexNumber[] output = Transform(false, signal);
            return output.Select(x => x.X).ToArray();
        }

        private static ComplexNumber[] Transform(bool forward, ComplexNumber[] signal)
        {
            float sign = forward ? -1 : +1;
            ComplexNumber[] result = new ComplexNumber[signal.Length];
            for (int k = 0; k < signal.Length; k++)
            {
                ComplexNumber ck = new ComplexNumber(0, 0);
                for (int j = 0; j < signal.Length; j++)
                {
                    ck += signal[j] * new ComplexNumber(+(float)Math.Cos(2 * Math.PI * k * j / signal.Length),
                                                        sign * (float)Math.Sin(2 * Math.PI * k * j / signal.Length));
                }
                result[k] = ck;
            }

            if (forward)
            {
                for (int k = 0; k < signal.Length; k++)
                {
                    result[k].X /= signal.Length;
                    result[k].Y /= signal.Length;
                }
            }

            return result;
        }
    }
}
