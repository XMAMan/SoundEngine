using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MusicMachine.Views.SequenzerVM.SequenzerCanvasElements
{
    class BackgroundRectangleCreator
    {
        private SequenzerCanvas canvas;
        private List<Rectangle> horizontalRecs = new List<Rectangle>();
        private List<Rectangle> verticalRecs = new List<Rectangle>();

        public BackgroundRectangleCreator(SequenzerCanvas canvas)
        {
            this.canvas = canvas;

            //Füge horizontale Balken dem Canvas hinzu
            this.canvas.PropertyChanged
                .Where(e => //Alle Parameter, welche bei Subscribe verwendet werden, werden hier aufgelistet
                    e == SequenzerCanvas.SC.ActualWidth ||
                    e == SequenzerCanvas.SC.ActualHeight ||
                    e == SequenzerCanvas.SC.MinToneIndex ||
                    e == SequenzerCanvas.SC.MaxToneIndex
                    )
                .Subscribe(x =>
                {
                    foreach (var rec in this.horizontalRecs) this.canvas.Children.Remove(rec);

                    //Horizontale Striche, welche die weißen Tasten markieren
                    double height = PianoViewHelper.KeyHeight(this.canvas.ActualHeight, this.canvas.MinToneIndex, this.canvas.MaxToneIndex);
                    for (int i = this.canvas.MinToneIndex; i < this.canvas.MaxToneIndex; i++)
                    {
                        int upperEdge = PianoViewHelper.StrichIndex(i) - PianoViewHelper.StrichIndex(this.canvas.MinToneIndex);

                        if (PianoViewHelper.IsBlackKey(i) == false && PianoViewHelper.GetKeyNumber(i) % 2 == 0)
                        {
                            double yPos = (this.canvas.ActualHeight - height * 2) - upperEdge * height;

                            var rec = CreateRectangle(new Point(0, yPos), new Size(this.canvas.ActualWidth, height * 2), 0, Brushes.White);
                            this.horizontalRecs.Add(rec);
                            this.canvas.Children.Add(rec);
                        }
                    }
                });


            //Füge vertikale Balken dem Canvas hinzu
            this.canvas.PropertyChanged
                .Where(e => //Alle Parameter, welche bei Subscribe verwendet werden, werden hier aufgelistet
                    e == SequenzerCanvas.SC.ActualWidth ||
                    e == SequenzerCanvas.SC.ActualHeight ||
                    e == SequenzerCanvas.SC.MaxSampleIndex ||
                    e == SequenzerCanvas.SC.MinSampleLengthForNewNotes ||
                    e == SequenzerCanvas.SC.SnapToGrid
                    )
                .Subscribe(x =>
                {
                    foreach (var rec in this.verticalRecs) this.canvas.Children.Remove(rec);

                    //Vertikale Striche, welche die kürzeste Note anzeigen
                    if (this.canvas.SnapToGrid)
                    {
                        int verticalLineCount = this.canvas.MaxSampleIndex / this.canvas.MinSampleLengthForNewNotes;
                        double verticalLineWidth = this.canvas.ActualWidth / verticalLineCount;
                        for (int i = 0; i < verticalLineCount; i++)
                        {
                            if (i % 2 == 0) continue;

                            var rec = CreateRectangle(new Point(i * verticalLineWidth, 0), new Size(verticalLineWidth, this.canvas.ActualHeight), -1, Brushes.LightBlue);
                            this.verticalRecs.Add(rec);
                            this.canvas.Children.Add(rec);
                        }
                    }
                });
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
