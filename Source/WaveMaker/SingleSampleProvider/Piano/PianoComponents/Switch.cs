namespace WaveMaker.KeyboardComponents
{
    //Weiche, welche entweder A oder B durchläßt
    public class Switch : IPianoComponent
    {
        public enum SwitchValues { A, B}
        public SwitchValues SwitchValue = SwitchValues.A;

        private IPianoComponent A, B;

        public Switch(IPianoComponent A, IPianoComponent B)
        {
            this.A = A;
            this.B = B;
        }

        public float GetSample(KeySampleData data)
        {
            if (this.SwitchValue == SwitchValues.A)
                return this.A.GetSample(data);
            else
                return this.B.GetSample(data);
        }
    }
}
