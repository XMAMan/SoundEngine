namespace SignalAnalyser
{
    //Unterteilt ein Frequenzsignal in lauter Bänder
    public class FrequenceRibbons
    {
        public float[] Values { get; private set; }

        public FrequenceRibbons(float[] values)
        {
            Values = values;
        }

        public static FrequenceRibbons CreateEmpty(int ribbonCount)
        {
            return new FrequenceRibbons(new float[ribbonCount]);
        }

        //Erzeuge 2^numberOfRibbonsPow2 Bänder
        public FrequenceRibbons(FrequenzeSpaceSignal frequenzeSpaceSignal, int numberOfRibbonsPow2)
        {
            int ribbonCount = (int)Math.Pow(2, numberOfRibbonsPow2);

            int blockSize = frequenzeSpaceSignal.MaxFrequence / ribbonCount;

            List<float> ribbons = new List<float>();
            for (int i = 0; i < frequenzeSpaceSignal.MaxFrequence; i += blockSize)
            {
                double amplitudeSum = 0;
                for (int frequence = i; frequence < i + blockSize; frequence ++)
                {
                    float amplitude = frequenzeSpaceSignal.GetAmplitude(frequence);
                    amplitudeSum += Math.Abs(amplitude);
                }
                ribbons.Add((float)amplitudeSum);
            }

            this.Values = ribbons.ToArray();
        }
    }
}
