using MidiParser;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            text1.AllowDrop = true;
            text1.DragEnter += new DragEventHandler(textBox1_DragEnter);
            text1.Drop += new DragEventHandler(textBox1_DragDrop);
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
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