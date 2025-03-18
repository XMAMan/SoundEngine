using MusicMachine.Controls.SequenzerElements.Piano;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WaveMaker.Sequenzer;

namespace MusicMachine.Controls.SequenzerElements.Sequenzer
{
    interface INoteRectangleProvider
    {
        Rectangle[] NoteRecs { get; }
    }

    public class RectangleNotes : INoteRectangleProvider
    {
        private SequenzerCanvas canvas;

        //Event für Erzeugen neuer Noten
        public class MouseNoteEventArgs
        {
            public int SamplePosition;
            public byte ToneIndex;
            public int SampleLength;
        }
        public delegate void CreateNoteHandler(object sender, MouseNoteEventArgs eventArgs);
        public event CreateNoteHandler CreateNoteEvent;

        public Rectangle[] NoteRecs { get; private set; } = new Rectangle[0];

        public RectangleNotes(SequenzerCanvas canvas)
        {
            this.canvas = canvas;

            

            
            //Erzeuge Noten
            this.canvas.PropertyChanged
                .Where(e => //Alle Parameter, welche bei Subscribe verwendet werden, werden hier aufgelistet
                    e == SequenzerCanvas.SC.Notes
                    )
                .Subscribe(x =>
                {
                    foreach (var rec in this.NoteRecs) this.canvas.Children.Remove(rec);

                    this.NoteRecs = CreateRectangles();
                    foreach (var rec in this.NoteRecs) this.canvas.Children.Add(rec);
                });

            //Verändere Größe/Position der Noten
            this.canvas.PropertyChanged
                .Where(e => //Alle Parameter, welche bei Subscribe verwendet werden, werden hier aufgelistet
                    e == SequenzerCanvas.SC.ActualWidth ||
                    e == SequenzerCanvas.SC.ActualHeight ||
                    e == SequenzerCanvas.SC.MinToneIndex ||
                    e == SequenzerCanvas.SC.MaxToneIndex ||
                    e == SequenzerCanvas.SC.MaxSampleIndex
                    )
                .Subscribe(x =>
                {
                    double height = PianoViewHelper.KeyHeight(this.canvas.ActualHeight, this.canvas.MinToneIndex, this.canvas.MaxToneIndex);
                    int minStrichIndex = PianoViewHelper.StrichIndex(this.canvas.MinToneIndex);
                    foreach (var rec in this.NoteRecs)
                    {
                        var viewModel = rec.DataContext as SequenzerNoteViewModel;
                        var p = GetPixPosForNote(viewModel.Model, height, minStrichIndex);
                        var s = GetNoteSize(viewModel.Model, height);

                        viewModel.LeftPosition = p.X;
                        viewModel.TopPosition = p.Y;
                        viewModel.Width = s.Width;
                        rec.Height = s.Height;
                    }
                });
        }

        public void AttacheToCanvasEvents()
        {
            this.canvas.MouseMove += SequenzerCanvas_MouseMove;
            this.canvas.MouseUp += SequenzerCanvas_MouseUp;
        }

        //Erzeuge neue Note bei Mausklick auf Canvas
        private void SequenzerCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && this.leftButtonIsDownOnNote == false)
            {
                var pos = e.GetPosition(this.canvas);
                if (this.canvas.SnapToGrid)
                {
                    int verticalLineCount = this.canvas.MaxSampleIndex / this.canvas.MinSampleLengthForNewNotes;
                    double verticalLineWidth = this.canvas.ActualWidth / verticalLineCount;
                    pos.X = (int)(pos.X / verticalLineWidth) * verticalLineWidth;
                }
                int samplePosition = Math.Max(0, (int)(pos.X / this.canvas.ActualWidth * this.canvas.MaxSampleIndex));
                int toneIndex = PianoViewHelper.GetToneIndexFromPixelPos(this.canvas.ActualHeight, this.canvas.MinToneIndex, this.canvas.MaxToneIndex, pos.Y);
                if (toneIndex != -1)
                    this.CreateNoteEvent?.Invoke(this, new MouseNoteEventArgs() { SamplePosition = samplePosition, ToneIndex = (byte)toneIndex, SampleLength = this.canvas.MinSampleLengthForNewNotes - 1 });

                e.Handled = true;
            }
        }

        private bool leftButtonIsDownOnNote = false;
        private Point relativeMouseOnRectangle = new Point(0, 0);
        private Rectangle movingRectangle = null;
        enum MouseRecState { OnCenter, OnLeftEdge, OnRightEdge }
        private MouseRecState mouseRecState = MouseRecState.OnCenter; //Verändert sich beim MouseMove-Event über dem Rechteck
        private double recWidth, recLeft; //Breite/Left zum MouseDown-Zeitpunkt

        private void SequenzerCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Released)
            {
                this.leftButtonIsDownOnNote = false;
            }

            //Verschiebe Note
            if (this.leftButtonIsDownOnNote && this.mouseRecState == MouseRecState.OnCenter)
            {
                var pos = e.GetPosition(this.canvas);
                if (this.canvas.SnapToGrid)
                {
                    int verticalLineCount = this.canvas.MaxSampleIndex / this.canvas.MinSampleLengthForNewNotes;
                    double verticalLineWidth = this.canvas.ActualWidth / verticalLineCount;
                    pos.X = (int)(pos.X / verticalLineWidth) * verticalLineWidth;
                }
                double newRecLeft = pos.X - this.relativeMouseOnRectangle.X;
                int samplePosition = (int)Math.Max(0, newRecLeft / this.canvas.ActualWidth * this.canvas.MaxSampleIndex);

                int toneIndex = PianoViewHelper.GetToneIndexFromPixelPos(this.canvas.ActualHeight, this.canvas.MinToneIndex, this.canvas.MaxToneIndex, pos.Y + this.relativeMouseOnRectangle.Y);
                if (toneIndex != -1)
                {
                    double height = PianoViewHelper.KeyHeight(this.canvas.ActualHeight, this.canvas.MinToneIndex, this.canvas.MaxToneIndex);
                    double upperEdge = (this.canvas.ActualHeight - height * 2) - (PianoViewHelper.StrichIndex(toneIndex) - PianoViewHelper.StrichIndex(this.canvas.MinToneIndex)) * height;
                    double left = samplePosition / (double)this.canvas.MaxSampleIndex * this.canvas.ActualWidth;
                    double top = upperEdge;

                    var viewModel = this.movingRectangle.DataContext as SequenzerNoteViewModel;

                    //Verschiebe alle markierten Noten mit
                    double deltaX = left - viewModel.LeftPosition;
                    double deltaY = top - viewModel.TopPosition;
                    foreach (var rec in this.NoteRecs)
                    {
                        if (rec == this.movingRectangle) continue;

                        var recVM = rec.DataContext as SequenzerNoteViewModel;
                        if (recVM.IsMarked == false) continue;
                        recVM.LeftPosition += deltaX;
                        recVM.TopPosition += deltaY;

                        int samplePosition1 = (int)Math.Max(0, recVM.LeftPosition / this.canvas.ActualWidth * this.canvas.MaxSampleIndex);
                        int toneIndex1 = PianoViewHelper.GetToneIndexFromPixelPos(this.canvas.ActualHeight, this.canvas.MinToneIndex, this.canvas.MaxToneIndex, recVM.TopPosition + this.relativeMouseOnRectangle.Y);

                        if (toneIndex1 != -1)
                        {
                            var eventArgs1 = new MouseNoteEventArgs() { SamplePosition = samplePosition1, ToneIndex = (byte)toneIndex1, SampleLength = recVM.Model.Length };
                            recVM.NoteMoveCommand.Execute(eventArgs1).Subscribe();
                        }                           
                    }

                    viewModel.LeftPosition = left;
                    viewModel.TopPosition = top;

                    var eventArgs = new MouseNoteEventArgs() { SamplePosition = samplePosition, ToneIndex = (byte)toneIndex, SampleLength = viewModel.Model.Length };
                    viewModel.NoteMoveCommand.Execute(eventArgs).Subscribe();
                }

                e.Handled = true;
            }

            //Vergrößere Note nach Links
            if (this.leftButtonIsDownOnNote && this.mouseRecState == MouseRecState.OnLeftEdge)
            {
                var pos = e.GetPosition(this.canvas);
                if (this.canvas.SnapToGrid)
                {
                    int verticalLineCount = this.canvas.MaxSampleIndex / this.canvas.MinSampleLengthForNewNotes;
                    double verticalLineWidth = this.canvas.ActualWidth / verticalLineCount;
                    pos.X = (int)(pos.X / verticalLineWidth) * verticalLineWidth + this.relativeMouseOnRectangle.X;
                }
                double oldRecRight = this.recLeft + this.recWidth;
                double newRecLeft = Math.Min(oldRecRight - 1, pos.X - this.relativeMouseOnRectangle.X);

                double newRecWidth = Math.Max(1, oldRecRight - newRecLeft);

                var viewModel = this.movingRectangle.DataContext as SequenzerNoteViewModel;

                //Vergrößere alle markierten Noten mit
                double deltaLeft = newRecLeft - viewModel.LeftPosition;
                double deltaWidth = newRecWidth - viewModel.Width;
                foreach (var rec in this.NoteRecs)
                {
                    if (rec == this.movingRectangle) continue;

                    var recVM = rec.DataContext as SequenzerNoteViewModel;
                    if (recVM.IsMarked == false) continue;
                    recVM.LeftPosition += deltaLeft;
                    recVM.Width = Math.Max(1, recVM.Width + deltaWidth);

                    int samplePosition1 = (int)Math.Max(0, recVM.LeftPosition / this.canvas.ActualWidth * this.canvas.MaxSampleIndex);
                    int sampleLength1 = (int)Math.Max(1, recVM.Width / this.canvas.ActualWidth * this.canvas.MaxSampleIndex - 1);

                    var eventArgs1 = new MouseNoteEventArgs() { SamplePosition = samplePosition1, ToneIndex = recVM.Model.NoteNumber, SampleLength = sampleLength1 };
                    recVM.NoteMoveCommand.Execute(eventArgs1).Subscribe();
                }

                viewModel.LeftPosition = newRecLeft;
                viewModel.Width = newRecWidth;

                int samplePosition = (int)Math.Max(0, viewModel.LeftPosition / this.canvas.ActualWidth * this.canvas.MaxSampleIndex);

                int sampleLength = (int)Math.Max(1, viewModel.Width / this.canvas.ActualWidth * this.canvas.MaxSampleIndex - 1);
                var eventArgs = new MouseNoteEventArgs() { SamplePosition = samplePosition, ToneIndex = viewModel.Model.NoteNumber, SampleLength = sampleLength };
                viewModel.NoteMoveCommand.Execute(eventArgs).Subscribe();

                e.Handled = true;
            }

            //Vergrößere Note nach Rechts
            if (this.leftButtonIsDownOnNote && this.mouseRecState == MouseRecState.OnRightEdge)
            {
                var pos = e.GetPosition(this.canvas);
                double newRecRight = pos.X - this.relativeMouseOnRectangle.X + this.recWidth;
                if (this.canvas.SnapToGrid)
                {
                    int verticalLineCount = this.canvas.MaxSampleIndex / this.canvas.MinSampleLengthForNewNotes;
                    double verticalLineWidth = this.canvas.ActualWidth / verticalLineCount;
                    newRecRight = (int)(newRecRight / verticalLineWidth) * verticalLineWidth;
                }
                double newRecWidth = Math.Max(1, newRecRight - this.recLeft);

                var viewModel = this.movingRectangle.DataContext as SequenzerNoteViewModel;

                //Vergrößere alle markierten Noten mit
                double deltaWidth = newRecWidth - viewModel.Width;
                foreach (var rec in this.NoteRecs)
                {
                    if (rec == this.movingRectangle) continue;

                    var recVM = rec.DataContext as SequenzerNoteViewModel;
                    if (recVM.IsMarked == false) continue;
                    recVM.Width = Math.Max(1, recVM.Width + deltaWidth);
                    
                    int samplePosition1 = (int)Math.Max(0, recVM.LeftPosition / this.canvas.ActualWidth * this.canvas.MaxSampleIndex);
                    int sampleLength1 = (int)Math.Max(1, recVM.Width / this.canvas.ActualWidth * this.canvas.MaxSampleIndex - 1);

                    var eventArgs1 = new MouseNoteEventArgs() { SamplePosition = samplePosition1, ToneIndex = recVM.Model.NoteNumber, SampleLength = sampleLength1 };
                    recVM.NoteMoveCommand.Execute(eventArgs1).Subscribe();
                }

                viewModel.Width = newRecWidth;

                int samplePosition = (int)Math.Max(0, viewModel.LeftPosition / this.canvas.ActualWidth * this.canvas.MaxSampleIndex);
                int sampleLength = (int)Math.Max(1, viewModel.Width / this.canvas.ActualWidth * this.canvas.MaxSampleIndex - 1) ;
                var eventArgs = new MouseNoteEventArgs() { SamplePosition = samplePosition, ToneIndex = viewModel.Model.NoteNumber, SampleLength = sampleLength };
                viewModel.NoteMoveCommand.Execute(eventArgs).Subscribe();

                e.Handled = true;
            }
        }

        private void Rec_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.leftButtonIsDownOnNote = true;

                Point mouse = e.GetPosition(this.canvas);

                if (this.canvas.SnapToGrid)
                {
                    int verticalLineCount = this.canvas.MaxSampleIndex / this.canvas.MinSampleLengthForNewNotes;
                    double verticalLineWidth = this.canvas.ActualWidth / verticalLineCount;
                    mouse.X = (int)(mouse.X / verticalLineWidth) * verticalLineWidth;
                }

                double recLeft = Canvas.GetLeft(sender as Rectangle);
                double recTop = Canvas.GetTop(sender as Rectangle);

                this.relativeMouseOnRectangle = new Point(mouse.X - recLeft, mouse.Y - recTop);
                this.movingRectangle = sender as Rectangle;

                this.recLeft = Canvas.GetLeft(sender as Rectangle);
                this.recWidth = (sender as Rectangle).Width;

                e.Handled = true;
            }

            if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var viewModel = (sender as Rectangle).DataContext as SequenzerNoteViewModel;
                viewModel.NoteDeleteCommand.Execute().Subscribe();

                e.Handled = true;
            }
        }
        private void Rec_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Released)
            {
                this.leftButtonIsDownOnNote = false;

                var viewModel = (sender as Rectangle).DataContext as SequenzerNoteViewModel;
                viewModel.NoteMouseUpCommand.Execute().Subscribe();

                e.Handled = true;
            }
        }

        private void Rec_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Released)
            {
                var rec = sender as Rectangle;
                var pos = e.GetPosition(rec);
                if (pos.X < 5)
                    mouseRecState = MouseRecState.OnLeftEdge;
                else
                if (pos.X > rec.Width - 5)
                    mouseRecState = MouseRecState.OnRightEdge;
                else
                    mouseRecState = MouseRecState.OnCenter;

                if (mouseRecState == MouseRecState.OnCenter)
                    rec.Cursor = Cursors.Arrow;
                else
                    rec.Cursor = Cursors.SizeWE;
            }
        }
        
        private Rectangle[] CreateRectangles()
        {
            List<Rectangle> list = new List<Rectangle>();

            if (this.canvas.Notes == null) return list.ToArray();

            double height = PianoViewHelper.KeyHeight(this.canvas.ActualHeight, this.canvas.MinToneIndex, this.canvas.MaxToneIndex);
            int minStrichIndex = PianoViewHelper.StrichIndex(this.canvas.MinToneIndex);
            foreach (var note in this.canvas.Notes)
            {
                var p = GetPixPosForNote(note.Model, height, minStrichIndex);
                var s = GetNoteSize(note.Model, height);
                var rec = CreateNoteRectangle(p, s);
                list.Add(rec);

                rec.DataContext = note;
                BindColorWithFill(note, rec);
                BindLeftPosition(note, rec);
                BindTopPosition(note, rec);
                BindWidth(note, rec);

                note.LeftPosition = p.X;
                note.TopPosition = p.Y;
                note.Width = s.Width;

                rec.MouseDown += Rec_MouseDown;
                rec.MouseUp += Rec_MouseUp;
                rec.MouseMove += Rec_MouseMove;

                //rec.InputBindings.Add(new MouseBinding() { Gesture = new MouseGesture(MouseAction.LeftClick), Command = note.NoteClickCommand });  //Mouse-Click-Event mit Command verbinden
            }

            return list.ToArray();
        }

        //Position oben links von ein einzelnen Ton
        private Point GetPixPosForNote(SequenzerKey key, double height, int minStrichIndex)
        {
            int upperEdge = PianoViewHelper.StrichIndex(key.NoteNumber) - minStrichIndex;

            double x = key.StartByteIndex / (double)this.canvas.MaxSampleIndex * this.canvas.ActualWidth;
            double y = (this.canvas.ActualHeight - height * 2) - upperEdge * height;
            return new Point(x, y);
        }

        private Size GetNoteSize(SequenzerKey key, double height)
        {
            return new Size(key.Length / (double)this.canvas.MaxSampleIndex * this.canvas.ActualWidth, height * 2);
        }

        private Rectangle CreateNoteRectangle(Point p, Size s)
        {
            Rectangle rec = new Rectangle() { Width = s.Width, Height = s.Height, Fill = Brushes.Red, Stroke = Brushes.DarkRed };
            //Canvas.SetLeft(rec, p.X); //Wird über Property-Binding gesetzt
            //Canvas.SetTop(rec, p.Y);
            Canvas.SetZIndex(rec, 1);
            return rec;
        }

        //https://stackoverflow.com/questions/7525185/how-to-set-a-binding-in-code
        private static void BindColorWithFill(SequenzerNoteViewModel note, Rectangle rec)
        {
            Binding myBinding = new Binding("Color");
            myBinding.Source = note;
            myBinding.Mode = BindingMode.OneWay;
            myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(rec, Rectangle.FillProperty, myBinding);
        }

        private static void BindLeftPosition(SequenzerNoteViewModel note, Rectangle rec)
        {
            Binding myBinding = new Binding("LeftPosition");
            myBinding.Source = note;
            myBinding.Mode = BindingMode.OneWay;
            myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(rec, Canvas.LeftProperty, myBinding);
        }
        private static void BindTopPosition(SequenzerNoteViewModel note, Rectangle rec)
        {
            Binding myBinding = new Binding("TopPosition");
            myBinding.Source = note;
            myBinding.Mode = BindingMode.OneWay;
            myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(rec, Canvas.TopProperty, myBinding);
        }
        private static void BindWidth(SequenzerNoteViewModel note, Rectangle rec)
        {
            Binding myBinding = new Binding("Width");
            myBinding.Source = note;
            myBinding.Mode = BindingMode.OneWay;
            myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(rec, Rectangle.WidthProperty, myBinding);
        }
    }

}
