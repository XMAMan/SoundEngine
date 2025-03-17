using Microsoft.Win32;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace MusicMachine.Model
{
    //Standard-Dialog, welche vom Codebehind gerufen werden können
    public static class InteractionActions
    {
        //Input: Filter (openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";); Output: Dateiname von der Datei die geöffnet werden soll
        public static IDisposable RegisterOpenFileDialog(this Interaction<string, string> interaction)
        {
            return interaction
            .RegisterHandler(
                    async interactionContext =>
                    {
                        string fileName = await Task<string>.Run(() =>
                        {
                            OpenFileDialog openFileDialog = new OpenFileDialog();
                            openFileDialog.Filter = interactionContext.Input;
                            openFileDialog.InitialDirectory = System.IO.Path.GetFullPath(Environment.CurrentDirectory + FileNameHelper.DataDirectory);
                            if (openFileDialog.ShowDialog() == true)
                                return openFileDialog.FileName;
                            return null;
                        });

                        interactionContext.SetOutput(fileName); //Der erste der SetOutput benutzt, wird verwendet
                    });
        }

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
                            saveFileDialog.InitialDirectory = System.IO.Path.GetFullPath(Environment.CurrentDirectory + FileNameHelper.DataDirectory);
                            if (saveFileDialog.ShowDialog() == true)
                                return saveFileDialog.FileName;
                            return null;
                        });

                        interactionContext.SetOutput(fileName); //Der erste der SetOutput benutzt, wird verwendet
                    });
        }
    }
}
