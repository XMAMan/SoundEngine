using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MusicMachine.Controls.SynthesizerElements.AudioFileControl
{
    //Hilft beim Einladen einer Audiodatei und definiert die Start- und Endposition
    public class AudioFileCanvas : Canvas
    {
        private Line lineLeft;
        private Line lineRight;

        public AudioFileCanvas()
        {
            this.lineLeft = new Line() { X1 = 0, Y1 = 0, X2 = 0, Y2 = this.ActualHeight, Stroke = Brushes.Blue, StrokeThickness = 4 };
            Canvas.SetZIndex(this.lineLeft, 2);
            this.lineLeft.Cursor = Cursors.SizeWE;
            this.lineLeft.MouseDown += LineLeft_MouseDown;
            this.lineLeft.MouseUp += LineLeft_MouseUp;

            this.lineRight = new Line() { X1 = 0, Y1 = 0, X2 = 0, Y2 = this.ActualHeight, Stroke = Brushes.Blue, StrokeThickness = 4 };
            Canvas.SetZIndex(this.lineRight, 2);
            this.lineRight.Cursor = Cursors.SizeWE;
            this.lineRight.MouseDown += LineRight_MouseDown;
            this.lineRight.MouseUp += LineRight_MouseUp;

            this.MouseMove += AudioFileCanvas_MouseMove;
            this.MouseLeave += AudioFileCanvas_MouseLeave;

            this.Children.Add(this.lineLeft);
            this.Children.Add(this.lineRight);

            this.SizeChanged += AudioFileCanvas_SizeChanged;
        }

        private void AudioFileCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            this.isDownOnLeftLine = false;
            this.isDownOnRightLine = false;
        }

        private void AudioFileCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update();
        }

        private void AudioFileCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.isDownOnLeftLine)
            {
                this.LeftPositionInMilliseconds = (float)Math.Max(0, Math.Min(this.RightPositionInMilliseconds, e.GetPosition(this).X / this.ActualWidth * this.FileLengthInMilliseconds));
            }

            if (this.isDownOnRightLine)
            {
                this.RightPositionInMilliseconds = (float)Math.Min(this.FileLengthInMilliseconds, Math.Max(this.LeftPositionInMilliseconds, e.GetPosition(this).X / this.ActualWidth * this.FileLengthInMilliseconds));
            }
        }

        private bool isDownOnLeftLine = false;
        private void LineLeft_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.isDownOnLeftLine = true;
                e.Handled = true;
            }
        }
        private void LineLeft_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.isDownOnLeftLine = false;
                e.Handled = true;
            }
        }

        private bool isDownOnRightLine = false;
        private void LineRight_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.isDownOnRightLine = true;
                e.Handled = true;
            }
        }
        private void LineRight_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.isDownOnRightLine = false;
                e.Handled = true;
            }
        }

        private void Update()
        {
            if (this.FileLengthInMilliseconds == 0)
            {
                this.lineLeft.Visibility = Visibility.Hidden;
                this.lineRight.Visibility = Visibility.Hidden;
                return;
            }
            this.lineLeft.Visibility = Visibility.Visible;
            this.lineRight.Visibility = Visibility.Visible;

            this.lineLeft.X1 = this.lineLeft.X2 = this.LeftPositionInMilliseconds / this.FileLengthInMilliseconds * this.ActualWidth;
            this.lineRight.X1 = this.lineRight.X2 = this.RightPositionInMilliseconds / this.FileLengthInMilliseconds * this.ActualWidth;
            this.lineLeft.Y2 = this.lineRight.Y2 = this.ActualHeight;
        }

        public static readonly DependencyProperty LeftPositionInMillisecondsProperty =
            DependencyProperty.Register("LeftPositionInMilliseconds", typeof(float), typeof(AudioFileCanvas),
         new FrameworkPropertyMetadata(0.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeLeftPositionInMilliseconds)));
        private static void ChangeLeftPositionInMilliseconds(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as AudioFileCanvas).Update();
            }
        }
        public float LeftPositionInMilliseconds
        {
            get => (float)GetValue(LeftPositionInMillisecondsProperty);
            set => SetValue(LeftPositionInMillisecondsProperty, value);
        }



        public static readonly DependencyProperty RightPositionInMillisecondsProperty =
            DependencyProperty.Register("RightPositionInMilliseconds", typeof(float), typeof(AudioFileCanvas),
         new FrameworkPropertyMetadata(0.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeRightPositionInMilliseconds)));
        private static void ChangeRightPositionInMilliseconds(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as AudioFileCanvas).Update();
            }
        }
        public float RightPositionInMilliseconds
        {
            get => (float)GetValue(RightPositionInMillisecondsProperty);
            set => SetValue(RightPositionInMillisecondsProperty, value);
        }



        public static readonly DependencyProperty FileLengthInMillisecondsProperty =
            DependencyProperty.Register("FileLengthInMilliseconds", typeof(float), typeof(AudioFileCanvas),
         new FrameworkPropertyMetadata(0.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeFileLengthInMilliseconds)));
        private static void ChangeFileLengthInMilliseconds(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as AudioFileCanvas).Update();
            }
        }
        public float FileLengthInMilliseconds
        {
            get => (float)GetValue(FileLengthInMillisecondsProperty);
            set => SetValue(FileLengthInMillisecondsProperty, value);
        }
    }
}
