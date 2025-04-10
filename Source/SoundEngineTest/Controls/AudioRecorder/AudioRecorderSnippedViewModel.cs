﻿using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using SoundEngine.SoundSnippeds;
using System.Reactive;

namespace SoundEngineTest.Controls.AudioRecorder
{
    public class AudioRecorderSnippedViewModel : ReactiveObject
    {
        private IAudioRecorderSnipped snipp;
        public AudioRecorderSnippedViewModel(IAudioRecorderSnipped snipp)
        {
            this.snipp = snipp;

            this.snipp.IsRunningChanged = (isRunning) => { IsRunning = isRunning; };

            Play = ReactiveCommand.Create(() =>
            {
                this.snipp.Play();
            });
            Stop = ReactiveCommand.Create(() =>
            {
                this.snipp.Stop();
            });

            if (SignalSources.Any())
            {
                SelectedSignalSource = SignalSources.First();
            }
        }

        public IEnumerable<string> SignalSources
        {
            get
            {
                return snipp.GetAvailableDevices();
            }
        }

        public string SelectedSignalSource
        {
            get { return snipp.SelectedDevice; }
            set
            {
                snipp.SelectedDevice = value;
                this.RaisePropertyChanged(nameof(SelectedSignalSource));
            }
        }

        [Reactive] public bool IsRunning { get; private set; } = false;
        public ReactiveCommand<Unit, Unit> Play { get; private set; }
        public ReactiveCommand<Unit, Unit> Stop { get; private set; }
        public float Volume { get { return snipp.Volume; } set { snipp.Volume = value; } }

        public bool UseDelayEffect { get { return snipp.UseDelayEffect; } set { snipp.UseDelayEffect = value; } }
        public bool UseHallEffect { get { return snipp.UseHallEffect; } set { snipp.UseHallEffect = value; } }
        public bool UseGainEffect { get { return snipp.UseGainEffect; } set { snipp.UseGainEffect = value; } }
        public float Gain { get { return snipp.Gain; } set { snipp.Gain = value; } }
        public bool UsePitchEffect { get { return snipp.UsePitchEffect; } set { snipp.UsePitchEffect = value; } }
        public float PitchEffect { get { return snipp.PitchEffect; } set { snipp.PitchEffect = value; } }
        public bool UseVolumeLfo { get { return snipp.UseVolumeLfo; } set { snipp.UseVolumeLfo = value; } }
        public float VolumeLfoFrequency { get { return snipp.VolumeLfoFrequency; } set { snipp.VolumeLfoFrequency = value; } }
    }
}
