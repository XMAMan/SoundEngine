using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveMaker.KeyboardComponents
{
    public class SubOscilator : Oscillator, IPianoComponent
    {
        public SubOscilator(int sampleRate)
            :base(sampleRate)
        {
        }

        public override float GetSample(KeySampleData data)
        {
            return GetSample(data.SampleIndex, data.Frequency / 2, this.PusleWidth);
        }
    }
}
