using Splat;
using Splat.ReactiveUIExtensions;
using Autofac;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WaveMaker;

namespace MusicMachine
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //Locator.CurrentMutable.RegisterType<NAudioWaveMaker.AudioFileHandler, IAudioFileHandler>();
        }
    }


}
