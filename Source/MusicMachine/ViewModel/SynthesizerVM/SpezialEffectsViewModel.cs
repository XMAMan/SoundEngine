using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveMaker.KeyboardComponents;

namespace MusicMachine.ViewModel.SynthesizerVM
{
    public class SpezialEffectsViewModel : ReactiveObject
    {
        private Synthesizer model;
        public SpezialEffectsViewModel(Synthesizer model)
        {
            this.model = model;
        }

        public bool UseDelayEffekt
        {
            get { return this.model.UseDelayEffekt; }
            set { this.model.UseDelayEffekt = value; this.RaisePropertyChanged(nameof(UseDelayEffekt)); }
        }

        public bool UseHallEffekt
        {
            get { return this.model.UseHallEffekt; }
            set { this.model.UseHallEffekt = value; this.RaisePropertyChanged(nameof(UseHallEffekt)); }
        }
        
        public void SetAllSettings(SynthesizerData data)
        {
            this.UseDelayEffekt = data.UseDelayEffekt;
        }
    }
}
