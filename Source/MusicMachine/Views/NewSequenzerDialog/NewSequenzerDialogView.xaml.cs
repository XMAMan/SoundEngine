using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicMachine.Views.NewSequenzerDialog
{
    /// <summary>
    /// Interaktionslogik für NewSequenzerDialogView.xaml
    /// </summary>
    public partial class NewSequenzerDialogView : Window
    {
        public NewSequenzerDialogView()
        {
            InitializeComponent();

            this.OkButton.Click += OkButton_Click;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
