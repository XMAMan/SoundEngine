using MusicMachine.Controls.SequenzerElements.MultiSequenzer;
using NAudioWaveMaker;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using WaveMaker;
using WaveMaker.Sequenzer;

namespace MusicMachine.Controls.MainControl
{
    //1.4.2020: Das Abspielen von Midi-Dateien und selbst erstellten Sequenzer-Daten klappt nun. Speichern/Laden der Projektdatei auch.
    //          Nun beginnt das Refactoring. Dabei fallen mir folgende Sachen auf:
    //-Es gibt im ViewModel anscheinend folgende Property-Arten:
    //1: Property, welche direkt seine Daten ans Model weiter leitet. Aufs Model erfolgt ausschließlich schreibender Zugriff. Lesen nur bei der Initialisierung
    //Beispiel: AdsrEnvelopeViewModel
    //public float AttackTimeInMs
    //{
    //    get { return this.model.AttackTimeInMs; }
    //    set { this.model.AttackTimeInMs = value; this.RaisePropertyChanged(nameof(AttackTimeInMs)); 
    //}
    //Wenn ich so eine Property persistieren will, dann lass ich mir vom Model (Synthesizer) mit GetAllSettings() alle Werte geben
    //Zum Laden weise ich im ViewModel der Property ein Wert zu, um somit dem Model den Wert zuzuweisen
    //2: Property, welche als Signalgeber vom Child an Parent gilt
    //Beispiel: SequenzerViewModel->Subject<SequenzerViewModel> SequenzerModelWasChanged -> Damit wird dem MultiSequenzerViewModel gesagt, dass 
    //          nach Einfügen/Ändern einer Note die sequenzerWithLongestLength-Property im MultiSequnzer aktualisiert werden soll
    //->Diese Logik gehört ins Model. Der ISequenzer sollte den MultiSequenzer updaten
    //3: Property, welche auf Änderungen von der View wartet, um dann andere View-Elemente zu ändern (Model bleibt davon unberührt)
    //Beisspiel: SequenzerViewModel->ReactiveCommand<Unit, SequenzerViewModel> MouseClickCanvas { get; private set; }
    //           Wenn das SequenzerCanvas ein Mausklick-Event sendet, wird dieses benutzt, um den selektierte Sequenzer aus der Liste zu aktualisieren
    //4: ViewModel ist Parent von anderen ViewModel. Beispiel: SequenezrViewModel hat SynthesizerViewModel-Property
    //5. Property, die von mehreren Stellen von der View aus gebunden wird: Beispiel: MultiSequenzerViewModel.SelectedSequenzer (SelectedSequenzer.SynthesizerViewModel ist der DataContext für die SynthiSettings)
    //Lektion: Jede Property, welche nicht an die View gebunden ist, ist zu hinterfragen. Aufgabe vom ViewModel ist Commands und Property von der View zu binden und andere ViewModel zu kennen

    //2.4.2020: Ideen für die Soundengine: PlayMidiFile, StartPlayingMidiFile/StopPlayingMidiFile, PlayTone(Frequence), StartPlayingTone/StopPlayingTone, PlaySoundFile, StartPlayingSoundFile/StopPlayingSoundFile, StopAllSounds
    //3.4.2020: Ideen: Es gibt neben dem Oszilator noch ein SoundFile-Block, der die Samples liefert und das KeyUp-Signal sendet. So kann ich mit dem ToneIndex den Pitch-Effekt steuern
    //                Außerdem benötige ich noch ein SoundAnalyser, welcher Phasengang/Frequenzgang/Bodediagramm anzeigt
    //                Ein Midiplayer wäre noch gut, um zu zeigen: Es geht
    //                Bei Mausklick soll Piano Töne spielen und Tasten das auch anzeigen
    //11.04.2020: Das ganze Synthesizer-Projekt besteht aus 4 Wissensbereichen:
    //  1. Dieses C#-Program so zu schreiben, dass es schön aus sieht und die DSP-Formeln umsetzt
    //  2. Wissen darüber, welche Synti-Instrumente es gibt und wie ich sie mit mein (oder den Helm-Synthesizer) erzeuge
    //  3. Wissen darüber, wie man die Noten im Sequenzer anordnen muss, um Effekte(z.B. den Kurby-Plink-Effekt) zu erzeugen
    //  4. DSP und Digitale Filter-Wissen
    //12.04.2020: Baustein 1 ist fertig. Ich erstelle nun Baustein 2: C#\Forschungen\Digitale Signalverarbeitung_2020\Synthesizer_Handbuch.odt

    //https://www.youtube.com/watch?v=GI8UEI8JOyk -> How to make 8-bit Music - Stage 1 (The Basics)
    //https://www.bonedo.de/artikel/einzelansicht/crashkurs-synthesizer-und-sounddesign.html

    //Für den Chorus-Effekt benötigt man ein Allpassfilter (Phase wird abhängig von Frequenz verschoben)
    //https://llllllll.co/t/digital-allpass-filters/27398/2
    //https://ccrma.stanford.edu/~dattorro/ -> Hat Tutorials für Audio
    //https://ccrma.stanford.edu/~dattorro/EffectDesignPart1.pdf -> Part 1: Reverberator and Other Filters
    //https://ccrma.stanford.edu/~dattorro/EffectDesignPart2.pdf -> Part 2: Delay-Line Modulation and Chorus
    //https://ccrma.stanford.edu/~dattorro/EffectDesignPart3.pdf -> Part 3: Oscillators: Sinusoidal and Pseudonoise
    //https://ccrma.stanford.edu/~dattorro/DigitalTimesI.pdf     -> The Implementation Of Digital Filters For High Fidelity Audio
    //https://github.com/madronalabs/madronalib/blob/master/source/DSP/MLDSPFilters.h 

    //https://www.youtube.com/watch?v=GJRMmJnpknY -> Hier wird erklärt wie man ein Lied komponiert (Arkorde)
    public class MainViewModel : ReactiveObject, IDisposable
    {
        private IAudioPlayer audioPlayer;
        private IAudioRecorder audioRecorder;

        public MultiSequenzerViewModel MultiSequenzerViewModel { get; private set; }

        public MainViewModel()
        {
            int sampleRate = 44100 / 2; //Das hier ist der Vorgabewert, welcher dann an alle anderen Stellen durchgereicht wird (Könnte man als Globallen Einstellparameter auslagern)

            this.audioRecorder = new AudioRecorder(sampleRate);

            var multiSequenzer = new MultiSequenzer(sampleRate, this.audioRecorder);

            this.audioPlayer = new NAudioWaveMaker.AudioPlayer(multiSequenzer);
            this.audioPlayer.StartPlaying();

            this.MultiSequenzerViewModel = new MultiSequenzerViewModel(multiSequenzer, new NAudioWaveMaker.AudioFileHandler());
        }

        public void Dispose()
        {
            if (this.audioPlayer is IDisposable)
            {
                (this.audioPlayer as IDisposable).Dispose();
            }

            if (this.audioRecorder is IDisposable)
            {

               (this.audioRecorder as IDisposable).Dispose();
            }
        }
    }
}
