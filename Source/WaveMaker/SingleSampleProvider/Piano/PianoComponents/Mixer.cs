using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveMaker.KeyboardComponents
{
    //Two-Components-Mixer
    public class Mixer : IPianoComponent
    {
        private IPianoComponent A, B;

        public float VolumeA { get; set; } = 1;
        public float VolumeB { get; set; } = 1;

        public Mixer(IPianoComponent A, IPianoComponent B)
        {
            this.A = A;
            this.B = B;
        }

        public float GetSample(KeySampleData data)
        {
            return this.A.GetSample(data) * this.VolumeA + this.B.GetSample(data) * this.VolumeB;
        }
    }
}
