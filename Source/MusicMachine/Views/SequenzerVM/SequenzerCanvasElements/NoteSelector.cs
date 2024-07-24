using MusicMachine.ViewModel.SequenzerVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MusicMachine.Views.SequenzerVM.SequenzerCanvasElements
{
    //Ein von der Maus aufgespanntes Rechteck zum selektieren der Noten
    class NoteSelector
    {
        //Diese Zahl legt fest, bis zu welchen MouseDown-MouseUp-Abstand eine Note erzeugt wird und ab wann ein Rechteck gespannt wird
        public double SetMouseDownEventHandeltDistance { get; private set; } = 10;

        private SequenzerCanvas canvas;
        private Rectangle rectangle;
        private INoteRectangleProvider rectangleProvider;

        public NoteSelector(SequenzerCanvas canvas, INoteRectangleProvider rectangleProvider)
        {
            this.canvas = canvas;
            this.rectangleProvider = rectangleProvider;

            this.rectangle = new Rectangle() 
            {
                Width = 1, 
                Height = 1, 
                Fill = Brushes.Transparent, 
                Stroke = Brushes.Black,
                StrokeThickness = 5,
                StrokeDashArray = DoubleCollection.Parse("2 2"),
                Visibility = Visibility.Hidden
            };
            Canvas.SetZIndex(this.rectangle, 2);

            this.canvas.Children.Add(this.rectangle);
        }

        public void AttacheToCanvasEvents()
        {
            this.canvas.MouseDown += Canvas_MouseDown;
            this.canvas.MouseMove += Canvas_MouseMove;
            this.canvas.MouseUp += Canvas_MouseUp;
        }


        private Point? mouseDownPoint = null;
        private void Canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(this.canvas);
                this.mouseDownPoint = pos;

                //e.Handled = true;
            }
        }

        private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && mouseDownPoint != null)
            {
                var mousePos = e.GetPosition(this.canvas);

                Point pos = new Point(Math.Min(mousePos.X, this.mouseDownPoint.Value.X), Math.Min(mousePos.Y, this.mouseDownPoint.Value.Y));
                Size size = new Size(Math.Abs(mousePos.X - this.mouseDownPoint.Value.X), Math.Abs(mousePos.Y - this.mouseDownPoint.Value.Y));

                Canvas.SetLeft(this.rectangle, pos.X);
                Canvas.SetTop(this.rectangle, pos.Y);
                this.rectangle.Width = size.Width;
                this.rectangle.Height = size.Height;
                this.rectangle.Visibility = Visibility.Visible;

                SetMarkedFlagFromAllFoundNotes();

                e.Handled = true;
            }
        }

        private void SetMarkedFlagFromAllFoundNotes()
        {
            double left1 = Canvas.GetLeft(this.rectangle);
            double top1 = Canvas.GetTop(this.rectangle);
            double right1 = left1 + this.rectangle.Width;
            double bottom1 = top1 + this.rectangle.Height;

            foreach (var rec in this.rectangleProvider.NoteRecs)
            {
                double left2 = Canvas.GetLeft(rec);
                double top2 = Canvas.GetTop(rec);
                double right2 = left2 + rec.Width;
                double bottom2 = top2 + rec.Height;

                bool intersect = right1 > left2 && bottom1 > top2 && top1 < bottom2 && left1 < right2;

                var viewModel = rec.DataContext as SequenzerNoteViewModel;
                viewModel.IsMarked = intersect;
            }
        }

        private void Canvas_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var mousePos = e.GetPosition(this.canvas);

                if (this.mouseDownPoint != null)
                {
                    double dx = mousePos.X - this.mouseDownPoint.Value.X;
                    double dy = mousePos.Y - this.mouseDownPoint.Value.Y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    if (distance > this.SetMouseDownEventHandeltDistance)
                    {
                        e.Handled = true;
                    }   
                    else
                    {
                        bool hasSomeSelection = this.rectangleProvider.NoteRecs.Any(x => (x.DataContext as SequenzerNoteViewModel).IsMarked);
                        if (hasSomeSelection) e.Handled = true;

                        foreach (var rec in this.rectangleProvider.NoteRecs)
                        {
                            var viewModel = rec.DataContext as SequenzerNoteViewModel;
                            viewModel.IsMarked = false;
                        }
                    }
                }
                this.rectangle.Visibility = Visibility.Hidden;
                this.mouseDownPoint = null;
            }
        }
    }
}
