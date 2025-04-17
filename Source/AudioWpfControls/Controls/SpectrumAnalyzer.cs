using System.Windows;
using System.Windows.Media;

namespace AudioWpfControls.Controls
{
    public class SpectrumAnalyzer : FrameworkElement
    {
        private float[] lastValues = null;

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Brushes.Black, new Pen(Brushes.Black, 1), new Rect(0, 0, this.ActualWidth, this.ActualHeight));

            if (this.Values == null || this.Values.Length == 0)
                return;

            double border = this.Border;
            int steps = this.Steps;     //In so viele Schritte wird ein Value-Wert unterteilt. D.h. pro Säule hat man maximal 20 Kästchen

            double width = (this.ActualWidth - 2 * border) / this.Values.Length;
            double height = (this.ActualHeight - 2 * border) / steps;

            for (int i = 0; i < this.Values.Length; i++)
            {
                int ySteps = (int)(this.Values[i] * steps);
                ySteps++; //+1, damit die Säule auch wirklich sichtbar ist
                for (int j=0; j < ySteps; j++)
                {
                    DrawRectangle(drawingContext, i, j, width, height);
                }         
                
                if (this.lastValues != null && this.lastValues.Length == this.Values.Length)
                {
                    if (lastValues[i] > Values[i])
                    {
                        double top = lastValues[i] - this.MaxFallDown;
                        if (top < Values[i]) top = Values[i];
                        int yStepValue = (int)(top * steps);
                        DrawRectangle(drawingContext, i, yStepValue, width, height);

                        Values[i] = (float)top;
                    }
                }
            }

            this.lastValues = this.Values;
        }

        private void DrawRectangle(DrawingContext drawingContext, int i, int j, double width, double height)
        {
            double x = this.Border + i * width;
            double y = this.ActualHeight - this.Border - height * (j + 1);

            double progress = (double)(j + 1) / this.Steps; //0.0 bis 1.0
            var color = Color.FromArgb(255, (byte)(progress * 255), (byte)(255 - progress * 255), 0);
            var brush = new SolidColorBrush(color);

            drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 2), new Rect(x, y, width, height));
        }

        #region Values-Property
        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register("Values", typeof(float[]), typeof(SpectrumAnalyzer),
         new FrameworkPropertyMetadata((float[])null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeValues)));
        private static void ChangeValues(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as SpectrumAnalyzer).InvalidateVisual();
            }
        }
        public float[] Values
        {
            get => (float[])GetValue(ValuesProperty);
            set => SetValue(ValuesProperty, value);
        }
        #endregion

        #region Steps-Property
        public static readonly DependencyProperty StepsProperty =
            DependencyProperty.Register("Steps", typeof(int), typeof(SpectrumAnalyzer),
         new FrameworkPropertyMetadata((int)20, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeSteps)));
        private static void ChangeSteps(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as SpectrumAnalyzer).InvalidateVisual();
            }
        }
        public int Steps
        {
            get => (int)GetValue(StepsProperty);
            set => SetValue(StepsProperty, value);
        }
        #endregion

        #region Border-Property
        public static readonly DependencyProperty BorderProperty =
            DependencyProperty.Register("Border", typeof(double), typeof(SpectrumAnalyzer),
         new FrameworkPropertyMetadata((double)10, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeBorder)));
        private static void ChangeBorder(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as SpectrumAnalyzer).InvalidateVisual();
            }
        }
        public double Border
        {
            get => (double)GetValue(BorderProperty);
            set => SetValue(BorderProperty, value);
        }
        #endregion

        //Mit der Geschwindigkeit fällt das oberste Kächsten maximal runter. Werte: 0..1
        #region MaxFallDown-Property
        public static readonly DependencyProperty MaxFallDownProperty =
            DependencyProperty.Register("MaxFallDown", typeof(double), typeof(SpectrumAnalyzer),
         new FrameworkPropertyMetadata((double)0.05, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeMaxFallDown)));
        private static void ChangeMaxFallDown(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as SpectrumAnalyzer).InvalidateVisual();
            }
        }
        public double MaxFallDown
        {
            get => (double)GetValue(MaxFallDownProperty);
            set => SetValue(MaxFallDownProperty, value);
        }
        #endregion
    }
}
