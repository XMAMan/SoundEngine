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
    public class MouseToneRectangle
    {
        private Rectangle rectangle;
        private SequenzerCanvas canvas;

        public MouseToneRectangle(SequenzerCanvas canvas)
        {
            this.canvas = canvas;

            this.rectangle = CreateMouseToneIndexRectangle();

            this.canvas.MouseMove += SequenzerCanvas_MouseMove;
            this.canvas.MouseLeave += SequenzerCanvas_MouseLeave;

            //Verändere Position und Größe des Mouse-Rechtecks
            this.canvas.PropertyChanged
                .Where(e =>  //Alle Parameter, welche bei Subscribe verwendet werden, werden hier aufgelistet
                    e == SequenzerCanvas.SC.ActualWidth ||
                    e == SequenzerCanvas.SC.ActualHeight ||
                    e == SequenzerCanvas.SC.MouseToneIndex ||
                    e == SequenzerCanvas.SC.MinToneIndex ||
                    e == SequenzerCanvas.SC.MaxToneIndex
                    )
                .Subscribe(x =>
                {
                    if (this.canvas.MouseToneIndex == -1)
                    {
                        this.rectangle.Visibility = Visibility.Hidden;
                        return;
                    }
                    double height = PianoViewHelper.KeyHeight(this.canvas.ActualHeight, this.canvas.MinToneIndex, this.canvas.MaxToneIndex);
                    int upperEdge = PianoViewHelper.StrichIndex(this.canvas.MouseToneIndex) - PianoViewHelper.StrichIndex(this.canvas.MinToneIndex);
                    double yPos = (this.canvas.ActualHeight - height * 2) - upperEdge * height;

                    Canvas.SetTop(this.rectangle, yPos);
                    this.rectangle.Height = height * 2;
                    this.rectangle.Width = this.canvas.ActualWidth;
                    this.rectangle.Visibility = Visibility.Visible;
                });

            this.canvas.Children.Add(this.rectangle);
        }

        private void SequenzerCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Released)
            {
                var pos = e.GetPosition(this.canvas);
                int toneIndex = PianoViewHelper.GetToneIndexFromPixelPos(this.canvas.ActualHeight, this.canvas.MinToneIndex, this.canvas.MaxToneIndex, pos.Y);
                if (toneIndex != -1) this.canvas.MouseToneIndex = toneIndex;
            }
        }

        private void SequenzerCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            this.canvas.MouseToneIndex = -1;
        }

        private Rectangle CreateMouseToneIndexRectangle()
        {
            double height = PianoViewHelper.KeyHeight(this.canvas.ActualHeight, this.canvas.MinToneIndex, this.canvas.MaxToneIndex);
            var mouseToneRectangle = CreateRectangle(new Point(0, 0), new Size(this.canvas.ActualWidth, height * 2), 1, Brushes.Cyan);

            mouseToneRectangle.Fill = Brushes.Transparent;
            mouseToneRectangle.Stroke = Brushes.Green;
            mouseToneRectangle.StrokeThickness = 2;
            mouseToneRectangle.StrokeDashArray = DoubleCollection.Parse("4 4");
            mouseToneRectangle.Visibility = Visibility.Hidden;

            return mouseToneRectangle;
        }

        private Rectangle CreateRectangle(Point p, Size s, int zIndex, Brush color)
        {
            Rectangle rec = new Rectangle() { Width = s.Width, Height = s.Height, Fill = color, Stroke = color };
            Canvas.SetLeft(rec, p.X);
            Canvas.SetTop(rec, p.Y);
            Canvas.SetZIndex(rec, zIndex);
            return rec;
        }
    }
}
