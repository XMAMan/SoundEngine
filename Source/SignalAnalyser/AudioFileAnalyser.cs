using WaveMaker.KeyboardComponents;

namespace SignalAnalyser
{
    //Zerlegt ein Audiosignal in Frequenzbänder und erzeugt ein Lowpass-Signal
    public class AudioFileAnalyser
    {
        private const int PowBlockLength = 12;      //2^PowBlockLength = Blocklänge der für die FFT als Input dient
        private const int PowBlockMove = 4;         //2^(PowBlockLength - PowBlockMove) = Um so viel Samples wird der Block jeweils verschoben
        private const int NumberOfRibbonsPow2 = 9;  //2^NumberOfRibbonsPow2 = So viele Frequenzbänder werden erzeugt.
        private const float LowPassCutoff = 0.3f;   //Cutoff für den Lowpassfilter, der das lowPassSignal erzeugt
        private const float LowPassResonance = 10;  //Resonance für den Lowpassfilter, der das lowPassSignal erzeugt
        private const float LowPassScale = 0.5f;    //Skalierung des Lowpass-Signals, damit es in die Grafik passt
        private const int FreqSpaceMoveX = 25;      //Verschiebe das Frequenzsignal um 25*256 (290 ms) Samples nach rechts damit es mit dem LowPass-Signal übereinstimmt

        private float[] samples;
        private int sampleRate;
        private FrequenceRibbons[] freqSpaceSignal;
        private float[] lowPassSignalFromFilter;
        private float[] lowPassSignalFromFrequencySpace;

        public FrequenceRibbons[] GetFrequencySpaceSignal() => this.freqSpaceSignal;
        public float[] GetLowPassSignalFromFrequencySpace() => this.lowPassSignalFromFrequencySpace;
        public float[] GetLowPassSignalFromFilter() => lowPassSignalFromFilter;
        public float[] GetOriginalSignal() => samples;

        //time = 0..1
        public float[] GetFrequenceSpectrumFromTime(double time)
        {
            int index = (int)(time * freqSpaceSignal.Length);
            index = Math.Max(index, 0);
            index = Math.Min(index, freqSpaceSignal.Length - 1);
            return freqSpaceSignal[index].Values.ToArray();
        }

        //Erst werden 2^NumberOfRibbonsPow2 Signalbänder erzeugt.
        //Davon werden dann die ersten freqSpaceRange Signalbänder verwendet (die Restlichen Bänder werden summiert und sind dann das letzte Band)
        //Um das LowPass-Signal zu erzeugen, wird zum einen ein Filter verwendet und zum anderen wird die Summe der ersten lowPassRange Signalbänder gebildet.
        public AudioFileAnalyser(float[] samples, int sampelRate, int freqSpaceRange = 32, int lowPassRange = 5) 
        {
            this.samples = samples;
            this.sampleRate = sampelRate;

            //Teil 1: Erzeuge Signale laut den Konstanten
            this.freqSpaceSignal = GetRibbons(PowBlockLength, PowBlockMove, NumberOfRibbonsPow2).ToArray();
            this.lowPassSignalFromFilter = GetLowPassSignal(LowPassCutoff, LowPassResonance);

            //Verschiebe das Frequenzsignal um 25*256 (290 ms) Samples nach rechts damit es mit dem LowPass-Signal übereinstimmt
            //Warum es nun genau um 25 Schritte verschoben werden muss weiß ich nicht.
            this.freqSpaceSignal = MoveInXDirection(this.freqSpaceSignal, FreqSpaceMoveX);

            //Teil 2: Um Speicherplatz zu sparen: Nutze nur die ersten freqSpaceRange/lowPassRange Signalbänder
            this.freqSpaceSignal = freqSpaceSignal.Select(x => new FrequenceRibbons(GetSubrange(x.Values, freqSpaceRange).ToArray())).ToArray();
            this.lowPassSignalFromFrequencySpace = this.freqSpaceSignal.Select(x => x.Values.ToList().GetRange(0, lowPassRange).Sum()).ToArray();

            //Skaliere, damit die Frequenzsignale beim SpectrumAnalyzer genau von 0 bis 1 gehen
            ScaleByFreqSpaceSignal();
        }

        //Damit die oberen Frequenzbänder nicht einfach abgeschnitten werden bilde ich die Summe und lege sie in das letzte Frequenzband
        //Damit dieses Signal aber nicht zu groß in der Anzeige ist teile ich es noch durch 10 (Magic Number)
        private static float[] GetSubrange(float[] values, int range)
        {
            float[] newValues = values.ToList().GetRange(0, range).ToArray();
            float upperSum = values.ToList().GetRange(range, values.Length - range).Sum();
            newValues[newValues.Length - 1] = upperSum / 10;
            return newValues;
        }

        #region FrequenceSpace-Signal

        public void ScaleByFreqSpaceSignal()
        {
            float maxFreqValue = GetFrequencySpaceSignal().Max(x => x.Values.Max());
            ScaleData(1f / maxFreqValue);
        }

        public void ScaleByLowPassSignal()
        {
            float maxFreqValue = GetLowPassSignalFromFrequencySpace().Max(x => x);
            ScaleData(1f / maxFreqValue);
        }

        public void ScaleByLowPassAndFreqSpace()
        {
            //Skaliere, damit das Frequenzsignal genau von 0 bis 1 geht
            float maxFreqValue1 = GetLowPassSignalFromFrequencySpace().Max(x => x);
            float maxFreqValue2 = GetFrequencySpaceSignal().Max(x => x.Values.Max());
            ScaleData(1f / Math.Max(maxFreqValue1, maxFreqValue2));
        }

        public void ScaleData(float scaleY)
        {
            this.freqSpaceSignal = GetScaledFreqSpaceSignal(freqSpaceSignal, scaleY);
            this.lowPassSignalFromFilter = this.lowPassSignalFromFilter.Select(x => x * scaleY * LowPassScale).ToArray();
        }

        private static FrequenceRibbons[] MoveInXDirection(FrequenceRibbons[] freqSpaceSignal, int moveX)
        {
            List<FrequenceRibbons> moved = new List<FrequenceRibbons>();

            for (int i=0;i<moveX;i++)
            {
                moved.Add(FrequenceRibbons.CreateEmpty(freqSpaceSignal[0].Values.Length));
            }

            moved.AddRange(freqSpaceSignal.ToList().GetRange(0, freqSpaceSignal.Length - moveX));

            return moved.ToArray();
        }

        private static FrequenceRibbons[] GetScaledFreqSpaceSignal(FrequenceRibbons[] freqSpaceSignal, float scaleY)
        {
            List<FrequenceRibbons> scaled = new List<FrequenceRibbons>();

            //Gehe über alle Frequenzbänder
            for (int i = 0; i < freqSpaceSignal.Length; i++)
            {
                scaled.Add(new FrequenceRibbons(freqSpaceSignal[i].Values.Select(x => x * scaleY).ToArray()));
            }
            return scaled.ToArray();
        }

        //Zerlegt das Signal in lauter Blöcke, wo jeder Block 2^powBlockLength Samples hat.
        //Das Lesefenster wird um 2^(powBlockLength - powBlockMove) verschoben.

        //Die maximal erfasste Frequenz ist 2^(powBlockLength-1) Hz. Wenn diese Zahl zu hoch ist, erkennt man das Basssignal nicht mehr
        //powBlockMove = 1 (sehr ungenau) .. powBlockLength-1 (sehr genau) -> Um so mehr Beats per Minute ein Lied hat, um so höher muss dieser Wert sein
        private IEnumerable<FrequenceRibbons> GetRibbons(int powBlockLength = 12, int powBlockMove = 4, int numberOfRibbonsPow2 = 5)
        {
            foreach (var frequenzeSpaceSignal in GetFrequenzeSpaceSignals(powBlockLength, powBlockMove))
            {
                yield return new FrequenceRibbons(frequenzeSpaceSignal, numberOfRibbonsPow2);
            }
        }

        //zerlegt das Signal in lauter Blöcke, wo jeder Block 2^powBlockLength Samples hat
        //Für jeden dieser Blöcke wird dessen Frequencesignal erzeugt
        private IEnumerable<FrequenzeSpaceSignal> GetFrequenzeSpaceSignals(int powBlockLength, int powBlockMove)
        {
            foreach (var block in GetBlocks(powBlockLength, powBlockMove))
            {
                yield return FrequenzeSpaceSignal.CreateFromTimeSpace(block);
            }
        }

        //zerlegt das Signal in lauter Blöcke, wo jeder Block 2^powBlockLength Samples hat
        //beim letzten Block werden die restlichen Samples mit 0 aufgefüllt
        //Das Lesefenster wird um 2^(powBlockLength - powBlockMove) verschoben
        private IEnumerable<float[]> GetBlocks(int powBlockLength, int powBlockMove)
        {
            int blockLength = (int)Math.Pow(2, powBlockLength);
            int slidingWindow = (int)Math.Pow(2, powBlockMove);
            int index = 0;

            while (index < this.samples.Length)
            {
                int endIndex = Math.Min(index + blockLength, this.samples.Length);
                float[] block = new float[blockLength];
                Array.Copy(this.samples, index, block, 0, endIndex - index);
                yield return block;
                index += blockLength / slidingWindow;
            }
        }
        #endregion

        #region LowPass-Signal

        //cutOff = Geht von 0 bis 1
        //resonance = Verstärkungsfaktor nahe dem Cutoff. Zahl muss zwischen 1 und Wert > 1 liegen
        private float[] GetLowPassSignal(float cutOff, float resonance)
        {
            var signalSource = new AudioFile(this.sampleRate) { SampleData = this.samples };
            signalSource.LeftPositionInMilliseconds = 0;
            signalSource.RightPositionInMilliseconds = signalSource.GetFileLengthInMilliseconds();

            var lowPass = new Filter(signalSource, FilterType.LowPass, this.sampleRate);
            lowPass.IsEnabled = true;
            lowPass.CutOffFrequence = cutOff;
            lowPass.Resonance = resonance;

            KeySampleData key = new KeySampleData(float.NaN);

            float[] signal = new float[this.samples.Length];
            for (int i= 0; i < signal.Length; i++)
            {
                key.SampleIndex = i;
                signal[i] = lowPass.GetSample(key);
            }

            return signal;
        }
        #endregion
    }
}
