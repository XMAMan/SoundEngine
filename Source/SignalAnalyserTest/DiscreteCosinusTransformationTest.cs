using SignalAnalyser;
using System.Drawing;
using System.Text;

namespace SignalAnalyserTest
{
    public class DiscreteCosinusTransformationTest
    {
        private string WorkingDirectory = @"..\..\..\..\UnitTestResults\";

        [Fact]
        public void PlotFrequenceSpace()
        {
            int n = 32; //32 Abtastpunkte
            double[] signal = new double[n];
            for (int i = 0; i < n; i++)
            {
                double x = i / (double)n;
                //Signal ist Summe von 10 Sinusfunktionen mit den Frequenzen 0.1,1,2,3,...9
                double y = //Math.Cos(0 * Math.PI * 2 * x) * 0.1 +
                           Math.Cos(1 * Math.PI * 2 * x) * 1 +
                           Math.Cos(2 * Math.PI * 2 * x) * 2 +
                           Math.Cos(3 * Math.PI * 2 * x) * 3 +
                           Math.Cos(4 * Math.PI * 2 * x) * 4 +
                           Math.Cos(5 * Math.PI * 2 * x) * 5 +
                           Math.Cos(6 * Math.PI * 2 * x) * 6 +
                           Math.Cos(7 * Math.PI * 2 * x) * 7 +
                           Math.Cos(8 * Math.PI * 2 * x) * 8 +
                           Math.Cos(9 * Math.PI * 2 * x) * 9;

                signal[i] = y;
            }

            var freqSpace = DiscreteCosinusTransformation.TransformFromTimeToFrequenceSpace(signal.Select(x => (float)x).ToArray());
            var timespace = DiscreteCosinusTransformation.TransformFromFrequnceToTimeSpace(freqSpace);

            StringBuilder str = new StringBuilder();
            for (int i = 0; i < signal.Length; i++)
            {
                str.AppendLine("Differenze = " + Math.Abs(signal[i] - timespace[i]));
            }
            string result = str.ToString();


            Bitmap bild = new Bitmap(400, 200);
            Graphics grx = Graphics.FromImage(bild);
            for (int i = 0; i < signal.Length - 1; i++)
            {
                grx.DrawLine(Pens.Red, i * bild.Width / (float)signal.Length, (float)signal[i] * bild.Height / 2 + bild.Height / 2, (i + 1) * bild.Width / (float)signal.Length, (float)signal[i + 1] * bild.Height / 2 + bild.Height / 2);
                grx.DrawLine(Pens.Blue, i * bild.Width / (float)freqSpace.Length, (float)freqSpace[i] * bild.Height / 2 + bild.Height / 2, (i + 1) * bild.Width / (float)freqSpace.Length, (float)freqSpace[i + 1] * bild.Height / 2 + bild.Height / 2);
                grx.DrawLine(Pens.Orange, i * bild.Width / (float)timespace.Length, (float)timespace[i] * bild.Height / 2 + bild.Height / 2, (i + 1) * bild.Width / (float)timespace.Length, (float)timespace[i + 1] * bild.Height / 2 + bild.Height / 2);
            }
            for (int i = 0; i < signal.Length; i++)
            {
                grx.DrawEllipse(Pens.Green, i * bild.Width / (float)signal.Length - 2, (float)signal[i] * bild.Height / 2 + bild.Height / 2 - 2, 4, 4);
            }
            grx.Dispose();
            bild.Save(WorkingDirectory + "DiscreteCosinusTransform.bmp");
        }
    }
}
