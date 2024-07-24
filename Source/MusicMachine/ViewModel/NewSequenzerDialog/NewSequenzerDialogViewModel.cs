﻿using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MusicMachine.ViewModel.NewSequenzerDialog
{
    public class NewSequenzerDialogViewModel : ReactiveObject
    {
        public class DialogResult
        {
            public int MinOctave;
            public int MaxOctave;
            public int LengthInSeconds;
        }

        [Reactive] public int MinOctave { get; set; } = 2;
        [Reactive] public int MaxOctave { get; set; } = 6;
        [Reactive] public int LengthInSeconds { get; set; } = 25;


        public DialogResult GetResult()
        {
            return new DialogResult()
            {
                MinOctave = this.MinOctave,
                MaxOctave = this.MaxOctave,
                LengthInSeconds = this.LengthInSeconds,
            };
        }
    }
}