using System.Drawing;

namespace SignalAnalyser
{
    public static class SignalImageCreator
    {
        public static Color[] ColorArray = new Color[] { Color.Red, Color.Green, Color.Blue, Color.DarkRed, Color.DarkGreen, Color.DarkBlue, Color.DarkMagenta };

        public static Bitmap GetLowPassSignalImage(AudioFileAnalyser analyser, int width, int height)
        {
            //Skaliere, damit das LowPassSignal von 0 bis 1 geht
            float scale1Y = 1f / analyser.GetLowPassSignalFromFrequencySpace().Max();
            float scale2Y = 1f / analyser.GetLowPassSignalFromFilter().Max();

            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics grx = Graphics.FromImage(bitmap))
            {
                grx.Clear(Color.White);

                //PlotSingleSignalWithDots(grx, analyser.GetOriginalSignal(), Color.Red); //Das Originalsignal wird in Rot gezeichnet

                PlotSingleSignalWithDots(grx, analyser.GetLowPassSignalFromFilter().Select(x => x * scale2Y).ToArray(), Color.LightBlue);

                PlotSingleSignalWithLines(grx, analyser.GetLowPassSignalFromFrequencySpace().Select(x => x * scale1Y).ToArray(), Color.Red);

            }

            return bitmap;
        }

        public static Bitmap GetFrequencySpaceImage(AudioFileAnalyser analyser, int width, int height)
        {
            //Skaliere, damit das Frequenz-Signal von 0 bis 1 geht
            float scale1Y = 1f / analyser.GetFrequencySpaceSignal().Max(x => x.Values.Max());
            float scale2Y = 1f / analyser.GetLowPassSignalFromFilter().Max();

            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics grx = Graphics.FromImage(bitmap))
            {
                grx.Clear(Color.White);

                PlotSingleSignalWithDots(grx, analyser.GetLowPassSignalFromFilter().Select(x => x * scale2Y).ToArray(), Color.LightBlue);

                PlotFrequenceSignals(grx, analyser.GetFrequencySpaceSignal(), scale1Y);
            }

            return bitmap;
        }

        private static float[] GetSingleFrequenceRibbon(FrequenceRibbons[] ribbons, int index)
        {
            return ribbons.Select(x => x.Values[index]).ToArray();
        }

        private static void PlotFrequenceSignals(Graphics grx, FrequenceRibbons[] ribbons, float scaleY)
        {
            int ribbonCount = ribbons[0].Values.Length;

            //Gehe über alle Frequenzbänder
            for (int i=0; i < ribbonCount; i++)
            {
                float[] signal = GetSingleFrequenceRibbon(ribbons, i).Select(x => x * scaleY).ToArray();
                PlotSingleSignalWithLines(grx, signal, ColorArray[i % ColorArray.Length]);
            }
        }

        private static void PlotSingleSignalWithDots(Graphics grx, float[] signal, Color color)
        {
            var pen = new Pen(color, 2);

            for (int i = 0; i < signal.Length; i++)
            {
                float x1 = (Math.Max(0, i) / (float)signal.Length) * grx.VisibleClipBounds.Width;
                float y1 = signal[i] * grx.VisibleClipBounds.Height;

                grx.FillRectangle(new SolidBrush(color), x1 - 1, grx.VisibleClipBounds.Height - (y1 - 1), 2, 2);
            }
        }

        private static void PlotSingleSignalWithLines(Graphics grx, float[] signal, Color color)
        {
            var pen = new Pen(color, 2);

            for (int i = 0; i < signal.Length - 1; i++)
            {
                float x1 = (Math.Max(0, i) / (float)signal.Length) * grx.VisibleClipBounds.Width;
                float y1 = signal[i] * grx.VisibleClipBounds.Height;

                float x2 = (Math.Max(0, i + 1) / (float)signal.Length) * grx.VisibleClipBounds.Width;
                float y2 = signal[i + 1] * grx.VisibleClipBounds.Height;

                grx.DrawLine(pen, x1, grx.VisibleClipBounds.Height - y1, x2, grx.VisibleClipBounds.Height - y2);
            }
        }
    }
}
