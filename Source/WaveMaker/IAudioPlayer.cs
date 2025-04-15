namespace WaveMaker
{
    //Kapselt NAudio und andere Soundbibliotheken ab
    public interface IAudioPlayer
    {
        string[] GetAvailableDevices();
        string SelectedDevice { get; set; }
        void UseDefaultDevice();
        bool IsPlaying { get; }
        void StartPlaying(); //Startet ein Timer, welcher zyklisch nach neuen Audio-Daten fragt, um diese zur Soundkarte zu schicken
        void StopPlaying();

        //Wird zyklisch vom Timer gerufen, wenn er nach neuen Audiodaten fragt.
        //Kann benutzt werden, um Audiodaten aufzuzeichnen oder sie zu visualisieren
        public event EventHandler<float[]> AudioOutputCallback; 
    }
}
