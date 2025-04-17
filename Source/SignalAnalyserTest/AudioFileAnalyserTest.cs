using NAudioWaveMaker;
using SignalAnalyser;
using System.Drawing;
using System.Text;

namespace SignalAnalyserTest
{
    public class AudioFileAnalyserTest
    {
        private string ResultDirectory = @"..\..\..\..\UnitTestResults\";
        private string DataDirectory = @"..\..\..\..\..\Data\";
        private static Color[] ColorArray = new Color[] { Color.Red, Color.Green, Color.Blue, Color.DarkRed, Color.DarkGreen, Color.DarkBlue, Color.DarkMagenta };

        [Fact]
        public void ShowAudioFile1()
        {
            //Das Lied enthält 11 Trommelschläge
            int sampleRate = 44100 / 2;
            string audioFile = DataDirectory + "AnneKaffekanne.wma";
            float[] samples = GetSamplesFromAudioFile(audioFile, sampleRate);

            Bitmap bitmap = SignalImageCreator.GetLowPassSignalImage(new AudioFileAnalyser(samples, sampleRate), 800, 200);
            bitmap.Save(ResultDirectory + "AnneKaffekanne.png", System.Drawing.Imaging.ImageFormat.Png);

            //Testausgabe für OpenOffice Calc
            var freqSpaceSignals = new AudioFileAnalyser(samples, sampleRate).GetFrequencySpaceSignal();
            StringBuilder str = new StringBuilder();
            foreach (var ribbon in freqSpaceSignals)
            {
                str.AppendLine(string.Join("\t", ribbon.Values));
            }
            string result = str.ToString();
        }

        [Fact]
        public void ShowAudioFile2()
        {
            //Das Lied enthält 15 Trommelschläge
            int sampleRate = 44100 / 2;
            string audioFile = DataDirectory + "AlterTune - Love (DJ Chipstyler Remix).wma";
            float[] samples = GetSamplesFromAudioFile(audioFile, sampleRate);

            Bitmap bitmap = SignalImageCreator.GetFrequencySpaceImage(new AudioFileAnalyser(samples, sampleRate), 800, 200);
            bitmap.Save(ResultDirectory + "AlterTune - Love (DJ Chipstyler Remix).png", System.Drawing.Imaging.ImageFormat.Png);
        }

        private static float[] GetSamplesFromAudioFile(string audioFile, int sampleRate)
        {
            var reader = new AudioFileHandler();
            var samples = reader.GetSamplesFromAudioFile(audioFile, sampleRate);
            return samples;
        }       
    }
}
