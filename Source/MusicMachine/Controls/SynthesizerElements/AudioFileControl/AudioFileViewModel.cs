using Microsoft.Win32;
using MusicMachine.Helper;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.IO;
using System.Reactive;
using System.Windows.Media.Imaging;
using WaveMaker;
using WaveMaker.KeyboardComponents;

namespace MusicMachine.Controls.SynthesizerElements.AudioFileControl
{
    public class AudioFileViewModel : ReactiveObject
    {
        private Synthesizer model;
        private IAudioFileReader audioFileReader;
        public AudioFileViewModel(Synthesizer model, IAudioFileReader audioFileReader)
        {
            this.model = model;
            this.audioFileReader = audioFileReader;
            this.FileLengthInMilliseconds = this.model.AudioFile.GetFileLengthInMilliseconds();
            UpdateAudioImage();

            this.LoadAudioFileCommand = ReactiveCommand.Create(() => // Create a command that calls command A.
            {
                /*string fileName = await this.OpenFileDialog.Handle("mp3 (*.mp3)|*.mp3|Wave Format(*.wav)|*.wav|All files (*.*)|*.*");
                if (fileName != null)
                {
                    this.FileName = fileName;
                }*/

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "MusicFiles|*.wma;*.mp3;*.wav|wma (*.wma)|*.wma|mp3 (*.mp3)|*.mp3|Wave Format(*.wav)|*.wav|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = System.IO.Path.GetFullPath(Environment.CurrentDirectory + FileNameHelper.DataDirectory);
                if (openFileDialog.ShowDialog() == true)
                {
                    this.FileName = FileNameHelper.GetFileName(openFileDialog.FileName);
                    LoadAudioFile(FileNameHelper.GetPathRelativeToCurrentDirectory(openFileDialog.FileName));
                }
            });

            this.MouseDoubleClickOnPitchCommand = ReactiveCommand.Create(() =>
            {
                this.Pitch = 1;
            });

            this.MouseDoubleClickOnSpeedCommand = ReactiveCommand.Create(() =>
            {
                this.Speed = 1;
            });            
        }

        public void LoadAudioFile(string fileName)
        {
            this.model.AudioFileName = Path.GetFileName(fileName);
            this.model.AudioFileData = this.audioFileReader.GetSamplesFromAudioFile(fileName, model.AudioFile.SampleRate);
            this.FileLengthInMilliseconds = this.model.AudioFile.GetFileLengthInMilliseconds();
            this.LeftPositionInMilliseconds = 0;
            this.RightPositionInMilliseconds = this.FileLengthInMilliseconds;

            UpdateAudioImage();
        }

        private void UpdateAudioImage()
        {
            if (this.model.AudioFileData == null)
            {
                this.SampleImage = GraphicHelper.CreateFilledBitmap((int)this.ViewboxWidth, (int)this.ViewboxHeight, System.Drawing.Color.Yellow).ToBitmapImage();
                return;
            }
            System.Drawing.Bitmap image = new System.Drawing.Bitmap((int)this.ViewboxWidth, (int)this.ViewboxHeight);

            System.Drawing.Graphics grx = System.Drawing.Graphics.FromImage(image);
            grx.Clear(System.Drawing.Color.Yellow);

            for (int x = 0; x < image.Width; x++)
            {
                float s = this.model.AudioFileData[(int)(x / (double)this.ViewboxWidth * (this.model.AudioFileData.Length - 1))];
                s = Math.Abs(s) * 100;
                float h = (float)this.ViewboxHeight / 2;
                grx.DrawLine(System.Drawing.Pens.Red, x, h - h * s, x, h + h * s);
            }

            grx.Dispose();

            this.SampleImage = image.ToBitmapImage();
        }

        

        [Reactive] public double ViewboxWidth { get; set; } = 200;
        [Reactive] public double ViewboxHeight { get; set; } = 30;
        [Reactive] public BitmapImage SampleImage { get; set; }

        [Reactive] public float FileLengthInMilliseconds { get; set; } = 0;


        public string FileName
        {
            get { return this.model.AudioFileName; }
            set { this.model.AudioFileName = value; this.RaisePropertyChanged(nameof(FileName)); }
        }
        public float LeftPositionInMilliseconds
        {
            get { return this.model.LeftAudioFilePosition; }
            set { this.model.LeftAudioFilePosition = value; this.RaisePropertyChanged(nameof(LeftPositionInMilliseconds)); }
        }
        public float RightPositionInMilliseconds
        {
            get { return this.model.RightAudioFilePosition; }
            set { this.model.RightAudioFilePosition = value; this.RaisePropertyChanged(nameof(RightPositionInMilliseconds)); }
        }

        public float Pitch
        {
            get { return this.model.AudioFilePitch; }
            set { this.model.AudioFilePitch = value; this.RaisePropertyChanged(nameof(Pitch)); }
        }

        public float Speed
        {
            get { return this.model.AudioFileSpeed; }
            set { this.model.AudioFileSpeed = value; this.RaisePropertyChanged(nameof(Speed)); }
        }

        public ReactiveCommand<Unit, Unit> LoadAudioFileCommand { get; private set; }
        //public Interaction<string, string> OpenFileDialog { get; private set; } = new Interaction<string, string>(); //Input: Filter (openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";); Output: Dateiname von der Datei die geöffnet werden soll

        public ReactiveCommand<Unit, Unit> MouseDoubleClickOnPitchCommand { get; private set; }
        public ReactiveCommand<Unit, Unit> MouseDoubleClickOnSpeedCommand { get; private set; }

        public void SetAllSettings(SynthesizerData data, string searchDirectoryForAudioFiles)
        {
            this.FileName = data.AudioFileName;
            string fullAudioFileName = searchDirectoryForAudioFiles + "\\" + data.AudioFileName;
            if (File.Exists(fullAudioFileName))
            {
                this.model.AudioFileData = this.audioFileReader.GetSamplesFromAudioFile(fullAudioFileName, model.AudioFile.SampleRate);
            }            
            this.FileLengthInMilliseconds = this.model.AudioFile.GetFileLengthInMilliseconds();
            this.LeftPositionInMilliseconds = data.LeftAudioFilePosition;
            this.RightPositionInMilliseconds = data.RightAudioFilePosition;
            this.Pitch = data.AudioFilePitch;
            UpdateAudioImage();
        }
    }
}
