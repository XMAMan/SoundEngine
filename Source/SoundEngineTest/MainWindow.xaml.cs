﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SoundEngineTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModel model = new ViewModel();

        public MainWindow()
        {
            InitializeComponent();

            double l1 = (1 + Math.Sqrt(5)) / 2;
            double l2 = (1 - Math.Sqrt(5)) / 2;
            double l = 1 / Math.Sqrt(5);
            StringBuilder str = new StringBuilder();
            for (int i=0;i<100;i++)
            {
                double fn = l * (Math.Pow(l1, i) - Math.Pow(l2, i));
                str.AppendLine(i + ": " + fn);
                str.AppendLine(i + ": " + Math.Pow(l1, i));
                str.AppendLine(i + ": " + Math.Pow(l2, i));
            }
            string result = str.ToString();


            this.DataContext = model;

            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            this.model.Dispose();
        }
    }
}