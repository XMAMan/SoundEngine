using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MusicMachine.Controls.SequenzerElements.Piano
{
    public class PianoCanvas : Canvas
    {
        //Event für 'Taste wurde gedrückt'
        public delegate void PianoKeyDownHandler(object sender, int toneIndex);
        public event PianoKeyDownHandler PianoKeyDownEvent;

        //Event für 'Taste wurde losgelassen'
        public delegate void PianoKeyUpHandler(object sender, int toneIndex);
        public event PianoKeyUpHandler PianoKeyUpEvent;

        public PianoCanvas()
        {
            this.SizeChanged += PianoCanvas_SizeChanged;
        }

        private void UpdateData()
        {
            //if (this.MinToneIndex == 0 || this.MaxToneIndex == 127) return;

            this.Children.Clear();

            double height = PianoViewHelper.KeyHeight(this.ActualHeight, this.MinToneIndex, this.MaxToneIndex);
            for (int i=this.MinToneIndex;i < this.MaxToneIndex;i++)
            {
                bool isBlack = PianoViewHelper.IsBlackKey(i);
                int upperEdge = PianoViewHelper.StrichIndex(i) - PianoViewHelper.StrichIndex(this.MinToneIndex);
                Rectangle rec = null;
                if (isBlack)
                {
                    rec = AddRectangle(new Point(0.4 * this.ActualWidth, (this.ActualHeight - height * 2) - upperEdge * height), new Size(this.ActualWidth * 0.6, height * 2 + 1), 1, Brushes.Black, Brushes.Gray);
                    
                }
                else
                {
  
                    rec = AddRectangle(new Point(0, (this.ActualHeight - height * 2) - upperEdge * height), new Size(this.ActualWidth, height * 2 + 1), 0, Brushes.White, Brushes.Brown);

                    var text = AddText(new Point(2, (this.ActualHeight - height * 2) - upperEdge * height), PianoViewHelper.KeyName(i), height * 1.7);
                    text.DataContext = rec;
                    text.MouseDown += Text_MouseDown;
                    text.MouseMove += Text_MouseMove;
                    text.MouseUp += Text_MouseUp;
                    text.MouseLeave += Text_MouseLeave;
                }
                rec.DataContext = new KeyViewModel() { ToneIndex = i, KeyColor = rec.Fill }; //ToneIndex
                rec.MouseDown += Rec_MouseDown;
                rec.MouseMove += Rec_MouseMove;
                rec.MouseUp += Rec_MouseUp;
                rec.MouseLeave += Rec_MouseLeave;
            }
        }

        //Der TextBlock fängt die Mausevents leider ab. e.Handled auf false zu setzen bringt nichts; Mit PreviewMouse-Events beim Rectangle zu arbeiten hift auch nicht
        private void Text_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Rec_MouseDown((sender as FrameworkElement).DataContext, e);
        }
        private void Text_MouseMove(object sender, MouseEventArgs e)
        {
            Rec_MouseMove((sender as FrameworkElement).DataContext, e);
        }

        private void Text_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Rec_MouseUp((sender as FrameworkElement).DataContext, e);
        }
        private void Text_MouseLeave(object sender, MouseEventArgs e)
        {
            Rec_MouseLeave((sender as FrameworkElement).DataContext, e);
        }



        class KeyViewModel
        {
            public int ToneIndex;
            public bool IsPressed = false;
            public Brush KeyColor;
        }

        private void PlayTone(Rectangle rec)
        {
            KeyViewModel key = rec.DataContext as KeyViewModel;
            rec.Fill = Brushes.Blue;
            key.IsPressed = true;
            this.PianoKeyDownEvent?.Invoke(this, key.ToneIndex);
        }

        private void ReleaseTone(Rectangle rec)
        {
            KeyViewModel key = rec.DataContext as KeyViewModel;
            if (key.IsPressed)
            {
                key.IsPressed = false;
                rec.Fill = key.KeyColor;
                this.PianoKeyUpEvent?.Invoke(this, key.ToneIndex);
            }
        }

        private void Rec_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                PlayTone(sender as Rectangle);
            }                
        }
        private void Rec_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                PlayTone(sender as Rectangle);
            }
        }
        private void Rec_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                ReleaseTone(sender as Rectangle);
            }                
        }
        private void Rec_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ReleaseTone(sender as Rectangle);
        }


        private Rectangle AddRectangle(Point p, Size s, int zIndex, Brush fill, Brush stroke)
        {
            Rectangle rec = new Rectangle() { Width = s.Width, Height = s.Height, Fill = fill, Stroke = stroke };
            Canvas.SetLeft(rec, p.X);
            Canvas.SetTop(rec, p.Y);
            Canvas.SetZIndex(rec, zIndex);
            this.Children.Add(rec);
            return rec;
        }

        private TextBlock AddText(Point p, string text, double fontSize)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = Brushes.Black;
            textBlock.FontSize = fontSize;
            Canvas.SetLeft(textBlock, p.X);
            Canvas.SetTop(textBlock, p.Y);
            this.Children.Add(textBlock);
            return textBlock;
        }

        private void PianoCanvas_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            UpdateData();
        }

        public static readonly DependencyProperty MinToneIndexProperty =
            DependencyProperty.Register("MinToneIndex", typeof(byte), typeof(PianoCanvas),
         new FrameworkPropertyMetadata((byte)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeMinToneIndex)));
        private static void ChangeMinToneIndex(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as PianoCanvas).UpdateData();
            }
        }
        public byte MinToneIndex
        {
            get => (byte)GetValue(MinToneIndexProperty);
            set => SetValue(MinToneIndexProperty, value);
        }


        public static readonly DependencyProperty MaxToneIndexProperty =
            DependencyProperty.Register("MaxToneIndex", typeof(byte), typeof(PianoCanvas),
         new FrameworkPropertyMetadata((byte)127, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeMaxToneIndex)));
        private static void ChangeMaxToneIndex(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as PianoCanvas).UpdateData();
            }
        }
        public byte MaxToneIndex
        {
            get => (byte)GetValue(MaxToneIndexProperty);
            set => SetValue(MaxToneIndexProperty, value);
        }
    }
}
