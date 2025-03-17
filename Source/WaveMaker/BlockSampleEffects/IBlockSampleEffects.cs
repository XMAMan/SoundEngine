namespace WaveMaker.BlockSampleEffects
{
    //Bekommt als Input ein Array von Samples und gibt das veränderte Array zurück (Ist sows wie ein Digitaler Filter, der aber nicht Sample für Sample arbeitet sondern Blockweise)
    interface IBlockSampleEffects
    {
        float[] GetModifiSamples(float[] samples);
    }
}
