namespace SignalAnalyser
{
    class ComplexNumber
    {
        public float X { get; set; }
        public float Y { get; set; }

        public ComplexNumber(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public static ComplexNumber operator +(ComplexNumber c1, ComplexNumber c2)
        {
            return new ComplexNumber(c1.X + c2.X, c1.Y + c2.Y);
        }

        public static ComplexNumber operator -(ComplexNumber c1, ComplexNumber c2)
        {
            return new ComplexNumber(c1.X - c2.X, c1.Y - c2.Y);
        }

        public static ComplexNumber operator *(ComplexNumber c1, ComplexNumber c2)
        {
            return new ComplexNumber(c1.X * c2.X - c1.Y * c2.Y, c1.X * c2.Y + c1.Y * c2.X);
        }

        public override string ToString()
        {
            return "[" + this.X.ToString("F4") + " ; " + this.Y.ToString("F4") + "]";
            //return "[" + (this.X < 0.00001 ? "0" : this.X.ToString("0.0000")) + " ; " + (this.Y < 0.00001 ? "0" : this.Y.ToString("0.0000")) + "]";
        }
    }
}
