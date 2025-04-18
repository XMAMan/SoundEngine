﻿namespace SignalAnalyser
{
    static class FastDiskreteFourierTransform
    {
        //signal.Length must be a power of 2
        public static ComplexNumber[] TransformFromTimeToFrequenceSpace(float[] signal)
        {
            var input = signal.Select(x => new NAudio.Dsp.Complex() { X = (float)x, Y = 0 }).ToArray();
            NAudio.Dsp.FastFourierTransform.FFT(true, (int)Math.Log(signal.Length, 2.0), input);
            return input.Select(x => new ComplexNumber(x.X, x.Y)).ToArray();
        }

        public static float[] TransformFromFrequnceToTimeSpace(ComplexNumber[] signal)
        {
            var input = signal.Select(x => new NAudio.Dsp.Complex() { X = (float)x.X, Y = (float)x.Y }).ToArray();
            NAudio.Dsp.FastFourierTransform.FFT(false, (int)Math.Log(signal.Length, 2.0), input);
            return input.Select(x => (float)x.X).ToArray();
        }
    }
}
