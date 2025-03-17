using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MusicMachine.Views.SequenzerVM.SequenzerCanvasElements
{
    public class PlayLine
    {
        //Event für das verschieben des blauen Balken
        public delegate void MouseMoveSamplePositionHandler(object sender, int samplePosition);
        public event MouseMoveSamplePositionHandler MouseMoveSamplePositionEvent;

        private Line line;
        private SequenzerCanvas canvas;

        private bool leftButtonIsDownOnPlayline = false;

        public PlayLine(SequenzerCanvas canvas)
        {
            this.canvas = canvas;
            this.line = new Line() { X1 = 0, Y1 = 0, X2 = 0, Y2 = this.canvas.ActualHeight, Stroke = Brushes.Blue, StrokeThickness = 4 };
            Canvas.SetZIndex(this.line, 2);
            this.line.Cursor = Cursors.SizeWE;

            this.canvas.SizeChanged += SequenzerCanvas_SizeChanged;
            this.line.MouseDown += PlayLine_MouseDown;
            this.line.MouseUp += PlayLine_MouseUp;
            this.canvas.MouseMove += SequenzerCanvas_MouseMove;
            this.canvas.MouseUp += SequenzerCanvas_MouseUp;

            //Verändere Position der PlayLine
            this.canvas.PropertyChanged
                .Where(e => //Alle Parameter, welche bei Subscribe verwendet werden, werden hier aufgelistet
                    e == SequenzerCanvas.SC.ActualWidth ||
                    e == SequenzerCanvas.SC.PlayPosition ||
                    e == SequenzerCanvas.SC.MaxSampleIndex 
                    )
                .Subscribe(x =>
                {
                    this.line.X1 = this.line.X2 = this.canvas.PlayPosition / (double)this.canvas.MaxSampleIndex * this.canvas.ActualWidth;
                });

            this.canvas.Children.Add(this.line);
        }

        //Verändere Größe der PlayLine
        private void SequenzerCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.line.Y2 = this.canvas.ActualHeight;
        }

        private void PlayLine_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.leftButtonIsDownOnPlayline = true;
                e.Handled = true;
            }
        }

        private void SequenzerCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Released)
            {
                this.leftButtonIsDownOnPlayline = false;
            }

            //Verschiebe PlayPosition
            if (this.leftButtonIsDownOnPlayline)
            {
                int samplePosition = Math.Max(0, (int)(e.GetPosition(this.canvas).X / this.canvas.ActualWidth * this.canvas.MaxSampleIndex));
                this.MouseMoveSamplePositionEvent?.Invoke(this, samplePosition);
            }
        }

        private void PlayLine_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Released)
            {
                this.leftButtonIsDownOnPlayline = false;
                e.Handled = true;
            }
        }

        private void SequenzerCanvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Released)
            {
                this.leftButtonIsDownOnPlayline = false;
            }
        }
    }
}
