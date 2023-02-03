using parking_detector.Classes;
using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        private void LoadImg_Click(object sender, RoutedEventArgs e)
        {
            videoControl.detect.image = (Image<SixLabors.ImageSharp.PixelFormats.Rgb24>)SixLabors.ImageSharp.Image.Load("frame.jpg");
            videoControl.detect.PreprocessImage();
            videoControl.detect.RunInference();

            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(videoControl.detect.ViewPrediction());
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();
            videoControl.imageResult.Source = biImg as ImageSource;
            
        }
    }
}
