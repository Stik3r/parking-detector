using System;
using System.Collections.Generic;
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
using parking_detector.Classes;

namespace parking_detector.Controls
{
    /// <summary>
    /// Логика взаимодействия для ButtonsControl.xaml
    /// </summary>
    public partial class ButtonsControl : UserControl
    {
        public VideoControl videoControl;
        public ButtonsControl()
        {
            InitializeComponent();
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            string path = "";
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Video Files |*.avi;*mp4;";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                path = dialog.FileName;
            }

            videoControl.videoPlayer.Source = (Uri)(new UriTypeConverter().ConvertFromString(path));
            videoControl.videoPlayer.Play();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            videoControl.videoPlayer.Pause();
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            videoControl.videoPlayer.Play();
            Detection d = new Detection();
            MessageBox.Show(d.ToString());
        }
    }
}
