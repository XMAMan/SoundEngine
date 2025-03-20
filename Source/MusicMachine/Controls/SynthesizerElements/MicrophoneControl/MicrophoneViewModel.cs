using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using WaveMaker;

namespace MusicMachine.Controls.SynthesizerElements.MicrophoneControl
{
    public class MicrophoneViewModel : ReactiveObject
    {
        public static string StartRecord = @"pack://application:,,,/MusicMachine;component/Controls/SynthesizerElements/MicrophoneControl/StartRecord.png";
        public static string StopRecord = @"pack://application:,,,/MusicMachine;component/Controls/SynthesizerElements/MicrophoneControl/StopRecord.png";

        [Reactive] public ImageSource StartStopImage { get; set; } = new BitmapImage(new Uri(MicrophoneViewModel.StartRecord, UriKind.Absolute));

        private IAudioRecorder model;
        private ITestToneProvider testToneProvider;
        
        public MicrophoneViewModel(IAudioRecorder model, ITestToneProvider testToneProvider)
        {
            this.model = model;
            this.testToneProvider = testToneProvider;

            this.StartStopCommand = ReactiveCommand.Create(() =>
            {
                if (this.IsRecording)
                {
                    StopRecording();                    
                }
                else
                {
                    StartRecording();
                }

                //CanExecute: AudioSourceIsSelected muss true sein
            }, this.WhenAnyValue(x => x.AudioSourceIsSelected, (audioSourceIsSelected) => audioSourceIsSelected == true));

            if (this.SignalSources.Any())
            {
                this.SelectedSignalSource = this.SignalSources.First();
            }
        }

        public void StartRecording()
        {
            this.IsRecording = true;
            this.model.StartRecording();
            this.testToneProvider.StartPlayingTestTone(); //Starte die Wiederabe. Sonst nimmt man zwar auf aber man hört nichts

        }

        public void StopRecording()
        {
            this.IsRecording = false;
            this.model.StopRecording();
            this.testToneProvider.StopPlayingTestTone();
        }

        public IEnumerable<string> SignalSources
        {
            get
            {
                return this.model.GetAvailableDevices();
            }
        }

        public string SelectedSignalSource
        {
            get { return this.model.SelectedDevice; }
            set
            {
                this.model.SelectedDevice = value;
                this.RaisePropertyChanged(nameof(SelectedSignalSource));
                this.AudioSourceIsSelected = true;
            }
        }

        [Reactive] public bool AudioSourceIsSelected { get; set; } = false;

        private bool isRecording = false;
        public bool IsRecording
        {
            get => this.isRecording;
            set
            {
                this.isRecording = value;

                if (this.isRecording)
                    StartStopImage = new BitmapImage(new Uri(MicrophoneViewModel.StopRecord, UriKind.Absolute));
                else
                    StartStopImage = new BitmapImage(new Uri(MicrophoneViewModel.StartRecord, UriKind.Absolute));

                this.RaisePropertyChanged(nameof(IsRecording));
            }
        }

        public ReactiveCommand<Unit, Unit> StartStopCommand { get; private set; }

    }
}
