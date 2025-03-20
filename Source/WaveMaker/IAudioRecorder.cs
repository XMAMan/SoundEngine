namespace WaveMaker
{
    //Ließt Audio-Daten aus ein selektieren Device und speichert diese in einem Puffer um sie dann mit GetNextSample zurück zu geben
    public interface IAudioRecorder : ISingleSampleProvider
    {
        string[] GetAvailableDevices();
        string SelectedDevice { get; set; }
        void UseDefaultDevice();
        bool IsRecording { get; }
        void StartRecording();
        void StopRecording();        
    }
}
