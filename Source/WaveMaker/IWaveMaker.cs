namespace WaveMaker
{
    //Kapselt NAudio und andere Soundbibliotheken ab
    public interface IWaveMaker
    {
        void StartPlaying(); //Startet ein Timer, welcher zyklisch nach neuen Audio-Daten fragt, um diese zur Soundkarte zu schicken
    }
}
