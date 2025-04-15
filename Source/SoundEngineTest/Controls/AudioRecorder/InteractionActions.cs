using Microsoft.Win32;
using ReactiveUI;

namespace SoundEngineTest.Controls.AudioRecorder
{
    internal static class InteractionActions
    {
        //Input: Filter (openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";); Output: Dateiname von der Datei, die erzeugt werden soll
        public static IDisposable RegisterSaveFileDialog(this Interaction<string, string> interaction)
        {
            return interaction
            .RegisterHandler(
                    async interactionContext =>
                    {
                        string fileName = await Task<string>.Run(() =>
                        {
                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.Filter = interactionContext.Input;
                            saveFileDialog.InitialDirectory = System.IO.Path.GetFullPath(Environment.CurrentDirectory);
                            if (saveFileDialog.ShowDialog() == true)
                                return saveFileDialog.FileName;
                            return null;
                        });

                        interactionContext.SetOutput(fileName); //Der erste der SetOutput benutzt, wird verwendet
                    });
        }
    }
}
