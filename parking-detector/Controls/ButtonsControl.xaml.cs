using System;
using System.Windows;
using System.Windows.Controls;

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
        }
    }
}
