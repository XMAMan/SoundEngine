using MidiParser;
using System;
using System.Windows.Forms;

namespace MidiPlotter
{
    public partial class Form1 : Form
    {
        public Form1(string[] args)
        {
            InitializeComponent();

            textBox1.AllowDrop = true;

            if (args.Length > 0)
            {
                ShowMidiFile(args[0]);
            }
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            ShowMidiFile(files[0]);          
        }

        private void ShowMidiFile(string fileName)
        {
            this.Text = fileName;
            try

            {
                textBox1.Text = MidiFile.FromFile(fileName).TestAusgabe();
            }
            catch (Exception ex)
            {
                textBox1.Text = ex.ToString();
            }
        }
        
    }
}
