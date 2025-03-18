namespace WaveMaker.BlockSampleEffects
{
    internal class SpeedChanger : IBlockSampleEffects
    {
        public float Speed { get; set; } = 1; //0.0001f ... 100f

        public float[] GetModifiSamples(float[] samples)
        {
            float[] block = new float[(int)(samples.Length * Speed)];

            for (int i = 0; i < block.Length; i++)
            {
                block[i] = samples[(int)(i / Speed)];
            }

            return block;
        }
    }
}
