using MidiParser;
using System.Windows;

namespace MidiPlotter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Quelle für Drag & Drop-Handling für TextBox: https://xcalibursystems.com/wpf-drag-and-drop-textbox-for-windows-explorer-files/
            text1.AllowDrop = true;
            text1.PreviewDragEnter += new DragEventHandler(textBox1_DragEnter);
            text1.PreviewDragOver += new DragEventHandler(textBox1_DragOver);
            text1.PreviewDrop += new DragEventHandler(textBox1_DragDrop);
        }

        private void textBox1_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            ShowMidiFile(files[0]);
        }

        private void ShowMidiFile(string fileName)
        {
            text1.Text = fileName;
            try

            {
                text1.Text = MidiFile.FromFile(fileName).TestAusgabe();
            }
            catch (Exception ex)
            {
                text1.Text = ex.ToString();
            }
        }
    }
}