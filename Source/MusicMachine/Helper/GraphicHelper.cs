using System.IO;
using System.Windows.Media.Imaging;

namespace MusicMachine.Helper
{
    public static class GraphicHelper
    {
        //https://stackoverflow.com/questions/94456/load-a-wpf-bitmapimage-from-a-system-drawing-bitmap
        public static BitmapImage ToBitmapImage(this System.Drawing.Bitmap image)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                image.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        public static System.Drawing.Bitmap CreateFilledBitmap(int width, int height, System.Drawing.Color color)
        {
            System.Drawing.Bitmap image = new System.Drawing.Bitmap(width, height);
            System.Drawing.Graphics grx = System.Drawing.Graphics.FromImage(image);
            grx.Clear(System.Drawing.Color.Yellow);

            grx.Dispose();

            return image;
        }
    }
}
