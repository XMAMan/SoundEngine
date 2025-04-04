namespace SignalAnalyser
{
    //https://www.tu-chemnitz.de/informatik/ThIS/downloads/courses/ws02/datkom/Fouriertransformation.pdf
    public static class DiscreteCosinusTransformation //Wird bei Jpeg eingesetzt
    {
        public static float[] TransformFromTimeToFrequenceSpace(float[] signal)
        {
            return Dct2(signal);
        }

        public static float[] TransformFromFrequnceToTimeSpace(float[] signal)
        {
            return InverseDct2(signal);
        }

        //https://de.wikipedia.org/wiki/Diskrete_Kosinustransformation
        //Laut Wikipedia ist heißt diese Funktion hier DCT-II, welche ein Zeitsignal in ein Frequenzraum-Signal umwandlet
        private static float[] Dct2(float[] signal)
        {
            float[] result = new float[signal.Length];
            for (int k = 0; k < signal.Length; k++)
            {
                double sum = 0;
                for (int j = 0; j < signal.Length; j++)
                {
                    sum += signal[j] * Math.Cos(Math.PI * k * (j + 0.5) / signal.Length);
                }
                result[k] = (float)(sum);
            }

            return result;
        }

        private static float[] InverseDct2(float[] signal)
        {
            float[] result = new float[signal.Length];
            for (int k = 0; k < signal.Length; k++)
            {
                double sum = 0;
                for (int j = 1; j < signal.Length; j++)
                {
                    sum += signal[j] * Math.Cos(Math.PI * j * (k + 0.5) / signal.Length);
                }
                result[k] = (float)(0.5 * signal[0] + sum) * 2 / signal.Length;
            }

            return result;
        }
    }
}
