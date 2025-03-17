using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalAnalyser;
using System;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SignalAnalyserTest
{
    [TestClass]
    public class FrequenzeSpaceSignalTest
    {
        private string WorkingDirectory = @"..\..\..\UnitTestResults\";

        [TestMethod]
        public void PlotFrequenceSpace()
        {
            int n = 32; //32 Abtastpunkte
            double[] signal = new double[n];
            for (int i = 0; i < n; i++)
            {
                double x = i / (double)n;
                //Signal ist Summe von 10 Sinusfunktionen mit den Frequenzen 0.1,1,2,3,...9
                double y = Math.Sin(0 * Math.PI * 2 * x) * 0.1 +
                           Math.Sin(1 * Math.PI * 2 * x) * 1 +
                           Math.Sin(2 * Math.PI * 2 * x) * 2 +
                           Math.Sin(3 * Math.PI * 2 * x) * 3 +
                           Math.Sin(4 * Math.PI * 2 * x) * 4 +
                           Math.Sin(5 * Math.PI * 2 * x) * 5 +
                           Math.Sin(6 * Math.PI * 2 * x) * 6 +
                           Math.Sin(7 * Math.PI * 2 * x) * 7 +
                           Math.Sin(8 * Math.PI * 2 * x) * 8 +
                           Math.Sin(9 * Math.PI * 2 * x) * 9;

                signal[i] = y;
            }

            var freqSpace = FrequenzeSpaceSignal.CreateFromTimeSpace(signal.Select(x => (float)x).ToArray());

            StringBuilder str = new StringBuilder();
            for (int frequence = 0; frequence < freqSpace.MaxFrequence; frequence++)
            {
                float amplitude = freqSpace.GetAmplitude(frequence);
                str.AppendLine(frequence + " Hz -> " + amplitude);
            }
            string result = str.ToString();

            //Ausgabe: Ich habe die Sinussummen von 0 bis 9 Hz erzeugt und bekomme im Ergebnis auch nur für diese Werte angezeigt
            //0 Hz-> 0
            //1 Hz-> 0,9999998
            //2 Hz-> 2
            //3 Hz-> 3
            //4 Hz-> 4
            //5 Hz-> 5
            //6 Hz-> 6,000001
            //7 Hz-> 6,999999
            //8 Hz-> 8
            //9 Hz-> 8,999999
            //10 Hz-> 2,384186E-07
            //11 Hz-> 0
            //12 Hz-> 1,192093E-07
            //13 Hz-> 2,384186E-07
            //14 Hz-> 0
            //15 Hz-> - 1,490116E-07
        }

        [TestMethod]
        public void ModifiSignalInFrequenceSpace()
        {
            int n = 32;
            double[] signal = new double[n];            //Dieses Timespace-Signal will ich im Frequenz-Raum modifzizieren
            double[] modifiedSignal = new double[n];    //Das ist der Erwartungswert
            for (int i = 0; i < n; i++)
            {
                double x = i / (double)n;
                double y = Math.Sin(0 * Math.PI * 2 * x) * 0.1 +
                           Math.Sin(1 * Math.PI * 2 * x) * 1 +
                           Math.Sin(2 * Math.PI * 2 * x) * 2 +
                           Math.Sin(3 * Math.PI * 2 * x) * 3 +
                           Math.Sin(4 * Math.PI * 2 * x) * 4 +
                           Math.Sin(5 * Math.PI * 2 * x) * 5 +
                           Math.Sin(6 * Math.PI * 2 * x) * 6 +
                           Math.Sin(7 * Math.PI * 2 * x) * 7 +
                           Math.Sin(8 * Math.PI * 2 * x) * 8 +
                           Math.Sin(9 * Math.PI * 2 * x) * 9;

                double y1 = Math.Sin(0 * Math.PI * 2 * x) * 0.1 +
                           Math.Sin(1 * Math.PI * 2 * x) * 1 +
                           Math.Sin(2 * Math.PI * 2 * x) * 2 +
                           Math.Sin(3 * Math.PI * 2 * x) * 3 +
                           Math.Sin(4 * Math.PI * 2 * x) * 4 +
                           Math.Sin(5 * Math.PI * 2 * x) * 12 +     //Der 5 Hz-Anteil hat nun die Amplitude von 12 anstatt 5
                           Math.Sin(6 * Math.PI * 2 * x) * 6 +
                           Math.Sin(7 * Math.PI * 2 * x) * 7 +
                           Math.Sin(8 * Math.PI * 2 * x) * 8 +
                           Math.Sin(9 * Math.PI * 2 * x) * 9;
                signal[i] = y;
                modifiedSignal[i] = y1;
            }

            var freqSpace = FrequenzeSpaceSignal.CreateFromTimeSpace(signal.Select(x => (float)x).ToArray());
            freqSpace.SetAmplitude(5, 12); //Setze den 5-Hz-Anteil auf 12

            float[] timeSignalBack = freqSpace.ToTimeSpace();

            StringBuilder str = new StringBuilder();
            for (int frequence = 0; frequence < freqSpace.MaxFrequence; frequence++)
            {
                float amplitude = freqSpace.GetAmplitude(frequence);
                str.AppendLine(frequence + " Hz -> " + amplitude + "\t Differenze=" + Math.Abs(timeSignalBack[frequence] - modifiedSignal[frequence]).ToString("F4"));
            }
            string result = str.ToString();

            //Ausgabe: Die Differenz zwischen dem timeSignalBack(Istwert) und modifiedSignal(Sollwert) ist 0
            //0 Hz-> 0    Differenze = 0,0000
            //1 Hz-> 0,9999998    Differenze = 0,0000
            //2 Hz-> 2    Differenze = 0,0000
            //3 Hz-> 3    Differenze = 0,0000
            //4 Hz-> 4    Differenze = 0,0000
            //5 Hz-> 12   Differenze = 0,0000           -> Wurde von 5 auf 12 gesetzt
            //6 Hz-> 6,000001     Differenze = 0,0000
            //7 Hz-> 6,999999     Differenze = 0,0000
            //8 Hz-> 8    Differenze = 0,0000
            //9 Hz-> 8,999999     Differenze = 0,0000
            //10 Hz-> 2,384186E-07    Differenze = 0,0000
            //11 Hz-> 0   Differenze = 0,0000
            //12 Hz-> 1,192093E-07    Differenze = 0,0000
            //13 Hz-> 2,384186E-07    Differenze = 0,0000
            //14 Hz-> 0   Differenze = 0,0000
            //15 Hz-> - 1,490116E-07   Differenze = 0,0000
        }

        [TestMethod]
        public void CreateSawthouthSignalWithSinusSum()
        {
            //Gegeben ist eine Rechteck/Sägezahnkurve
            //Gesucht: Angenähertes Signal, was Summe aus Sinus-Funktionen ist
            int n = 32; //Abtastrate
            float frequence = 5; //MaxFrequenz ist n/2
            double[] signal = new double[n];
            for (int i = 0; i < n; i++)
            {
                double x = i / (double)n;

                float squreValue = (((i * 2 * frequence / n) % 2) - 1) > 0 ? 1 : -1;
                float sawToothValue = ((i * 2 * frequence / n) % 2) - 1;

                signal[i] = sawToothValue;
            }

            var freqSpace = FrequenzeSpaceSignal.CreateFromTimeSpace(signal.Select(x => (float)x).ToArray());

            //Weg 1 um angenähertes Signal zu erhalten: Inverse Fourier über ToTimeSpace()
            float[] timeSignal1 = freqSpace.ToTimeSpace();

            //Weg 2 um angenähertes Signal zu erhalten: Sinus-Summe über GetAmplitude()
            double[] timeSignal2 = new double[n];
            for (int i = 0; i < n; i++)
            {
                double x = i / (double)n;
                double sum = 0;
                for (int j = 1; j < n / 2; j++)//j ist die Frequence in Hz
                {
                    sum += Math.Sin(x * j * 2 * Math.PI) * freqSpace.GetAmplitude(j);//frequnceSignal[j].Y ist die Amplitude
                }
                timeSignal2[i] = sum;
            }

            //Ausgabe vom Vorgabesignal und der beiden Nährerungsfunktionen
            Bitmap bild = new Bitmap(400, 200);
            Graphics grx = Graphics.FromImage(bild);
            for (int i = 0; i < signal.Length - 1; i++)
            {
                grx.DrawLine(Pens.Red, i * bild.Width / (float)signal.Length, (float)signal[i] * bild.Height / 2 + bild.Height / 2, (i + 1) * bild.Width / (float)signal.Length, (float)signal[i + 1] * bild.Height / 2 + bild.Height / 2);
                grx.DrawLine(Pens.Blue, i * bild.Width / (float)timeSignal1.Length, (float)timeSignal1[i] * bild.Height / 2 + bild.Height / 2, (i + 1) * bild.Width / (float)timeSignal1.Length, (float)timeSignal1[i + 1] * bild.Height / 2 + bild.Height / 2);
                grx.DrawLine(Pens.Orange, i * bild.Width / (float)timeSignal2.Length, (float)timeSignal2[i] * bild.Height / 2 + bild.Height / 2, (i + 1) * bild.Width / (float)timeSignal2.Length, (float)timeSignal2[i + 1] * bild.Height / 2 + bild.Height / 2);
            }
            for (int i = 0; i < signal.Length; i++)
            {
                grx.DrawEllipse(Pens.Green, i * bild.Width / (float)signal.Length - 2, (float)signal[i] * bild.Height / 2 + bild.Height / 2 - 2, 4, 4);
            }
            grx.Dispose();
            bild.Save(WorkingDirectory + "SawthouthSignal.bmp");
        }
    }
}
