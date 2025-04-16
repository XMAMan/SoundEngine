using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AudioWpfControls.Converter
{
    //<Window.Resources>
    //    <local:ProgressForegroundConverter x:Key="ProgressForegroundConverter"/>
    //</Window.Resources>
    //<ProgressBar Width="100" Height="20" Minimum="0" Maximum="1" Value="{Binding OutputVolume}" Foreground="{Binding RelativeSource={RelativeSource Mode=Self}, Path=Value, Converter={StaticResource VolumeToColorConverter}}"/>
    //https://stackoverflow.com/questions/9413495/how-to-change-the-progressbar-foreground-color-based-on-its-value-depending-on-t
    public class VolumeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double progress = (double)value;

            progress = Math.Min(1, Math.Max(0, progress));

            return new SolidColorBrush(Color.FromArgb(255, (byte)(progress * 255), (byte)(255 - progress * 255), 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
