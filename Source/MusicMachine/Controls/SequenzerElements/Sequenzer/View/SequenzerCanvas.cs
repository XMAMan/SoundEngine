using MusicMachine.Controls.SequenzerElements.Piano;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MusicMachine.Controls.SequenzerElements.Sequenzer
{
    public class SequenzerCanvas : Canvas
    {
        //Das sind alle Property-Changed-Events, die der SequenzerCanvas feuert und die potentiell für seine Kind-Elemente wichtig sind
        public enum SC { ActualWidth, ActualHeight, Notes, MinToneIndex, MaxToneIndex, MaxSampleIndex, PlayPosition, IsEditable, MinSampleLengthForNewNotes, SnapToGrid, MouseToneIndex }

        //Das sind die Lieferanten für die Canvas-Childrens
        public PlayLine PlayLine { get; private set; }
        private MouseToneRectangle mouseToneRectangle { get; set; }
        private BackgroundRectangleCreator backgroundRectangleCreator;
        public RectangleNotes RectangleNotes { get; private set; }
        private NoteSelector noteSelector;


        public Subject<SC> PropertyChanged = new Subject<SC>();

        public SequenzerCanvas()
        {
            this.Background = Brushes.Beige;

            this.SizeChanged += SequenzerCanvas_SizeChanged;

            this.PlayLine = new PlayLine(this);
            this.mouseToneRectangle = new MouseToneRectangle(this);
            this.backgroundRectangleCreator = new BackgroundRectangleCreator(this);
            
            this.RectangleNotes = new RectangleNotes(this);
            this.noteSelector = new NoteSelector(this, this.RectangleNotes);

            //Erst muss der NoteSelector sich an die Maus-Events hängen, damit er das Event auf Handelt setzen kann, 
            //wenn ein Rechteck erzeugt wurde 
            //==> Verhindere, dass eien Note erzeugt wird, wenn jemand ein Selector-Rechteck aufspannt
            this.noteSelector.AttacheToCanvasEvents(); 
            this.RectangleNotes.AttacheToCanvasEvents();

        }

        private void SequenzerCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged) PropertyChanged.OnNext(SC.ActualWidth);
            if (e.HeightChanged) PropertyChanged.OnNext(SC.ActualHeight);
        }

        
        public static readonly DependencyProperty NotesProperty =
            DependencyProperty.Register("Notes", typeof(ObservableCollection<SequenzerNoteViewModel>), typeof(SequenzerCanvas),
         new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(ChangeNotes)));
        private static void ChangeNotes(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //https://stackoverflow.com/questions/4362278/observablecollection-dependency-property-does-not-update-when-item-in-collection
            if (e.OldValue != e.NewValue)
            {
                (d as SequenzerCanvas).PropertyChanged.OnNext(SC.Notes); //Liste wurde erzeugt (Mit initialen Werten)
            }

            if (e.OldValue != null)
            {
                var coll = (INotifyCollectionChanged)e.OldValue;
                coll.CollectionChanged -= (d as SequenzerCanvas).Notes_CollectionChanged; // Unsubscribe from CollectionChanged on the old collection
            }

            if (e.NewValue != null)
            {
                var coll = (INotifyCollectionChanged)e.NewValue;
                coll.CollectionChanged += (d as SequenzerCanvas).Notes_CollectionChanged; // Subscribe to CollectionChanged on the new collection
            }
        }
        //Neues Element wurde in Liste hinzugefügt
        private void Notes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // handle CollectionChanged
            PropertyChanged.OnNext(SC.Notes);
        }

        public ObservableCollection<SequenzerNoteViewModel> Notes
        {
            get => (ObservableCollection<SequenzerNoteViewModel>)GetValue(NotesProperty);
            set => SetValue(NotesProperty, value);
        }


        public static readonly DependencyProperty MinToneIndexProperty =
            DependencyProperty.Register("MinToneIndex", typeof(byte), typeof(SequenzerCanvas),
         new FrameworkPropertyMetadata((byte)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeMinToneIndex)));
        private static void ChangeMinToneIndex(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as SequenzerCanvas).PropertyChanged.OnNext(SC.MinToneIndex);
            }            
        }
        public byte MinToneIndex
        {
            get => (byte)GetValue(MinToneIndexProperty);
            set => SetValue(MinToneIndexProperty, value);
        }


        public static readonly DependencyProperty MaxToneIndexProperty =
            DependencyProperty.Register("MaxToneIndex", typeof(byte), typeof(SequenzerCanvas),
         new FrameworkPropertyMetadata((byte)127, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeMaxToneIndex)));
        private static void ChangeMaxToneIndex(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as SequenzerCanvas).PropertyChanged.OnNext(SC.MaxToneIndex);
            }
        }
        public byte MaxToneIndex
        {
            get => (byte)GetValue(MaxToneIndexProperty);
            set => SetValue(MaxToneIndexProperty, value);
        }


        public static readonly DependencyProperty MaxSampleIndexProperty =
            DependencyProperty.Register("MaxSampleIndex", typeof(int), typeof(SequenzerCanvas),
         new FrameworkPropertyMetadata(1000, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeMaxSampleIndex)));
        private static void ChangeMaxSampleIndex(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as SequenzerCanvas).PropertyChanged.OnNext(SC.MaxSampleIndex);
            }
        }
        public int MaxSampleIndex
        {
            get => (int)GetValue(MaxSampleIndexProperty);
            set => SetValue(MaxSampleIndexProperty, value);
        }


        public static readonly DependencyProperty PlayPositionProperty =
            DependencyProperty.Register("PlayPosition", typeof(int), typeof(SequenzerCanvas),
         new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangePlayPosition)));
        private static void ChangePlayPosition(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as SequenzerCanvas).PropertyChanged.OnNext(SC.PlayPosition);
            }
        }
        public int PlayPosition
        {
            get => (int)GetValue(PlayPositionProperty);
            set => SetValue(PlayPositionProperty, value);
        }


        //So viele Samples ist die kürzeste Note lang (Wenn ich selber neue Noten erzeuge)
        public static readonly DependencyProperty MinSampleLengthForNewNotesProperty =
            DependencyProperty.Register("MinSampleLengthForNewNotes", typeof(int), typeof(SequenzerCanvas),
         new FrameworkPropertyMetadata(5512, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeMinSampleLengthForNewNotes)));
        private static void ChangeMinSampleLengthForNewNotes(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as SequenzerCanvas).PropertyChanged.OnNext(SC.MinSampleLengthForNewNotes);
            }
        }
        public int MinSampleLengthForNewNotes
        {
            get => (int)GetValue(MinSampleLengthForNewNotesProperty);
            set => SetValue(MinSampleLengthForNewNotesProperty, value);
        }


        //Sollen neu erzeugte Noten am Vertikalen Gitter ausgerichtet werden?
        public static readonly DependencyProperty SnapToGridProperty =
            DependencyProperty.Register("SnapToGrid", typeof(bool), typeof(SequenzerCanvas),
         new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeSnapToGrid)));
        private static void ChangeSnapToGrid(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as SequenzerCanvas).PropertyChanged.OnNext(SC.SnapToGrid);
            }
        }
        public bool SnapToGrid
        {
            get => (bool)GetValue(SnapToGridProperty);
            set => SetValue(SnapToGridProperty, value);
        }



        public static readonly DependencyProperty MouseToneIndexProperty =
            DependencyProperty.Register("MouseToneIndex", typeof(int), typeof(SequenzerCanvas),
         new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ChangeMouseToneIndex)));
        private static void ChangeMouseToneIndex(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                (d as SequenzerCanvas).PropertyChanged.OnNext(SC.MouseToneIndex);
            }
        }
        public int MouseToneIndex
        {
            get => (int)GetValue(MouseToneIndexProperty);
            set => SetValue(MouseToneIndexProperty, value);
        }
    }
}
