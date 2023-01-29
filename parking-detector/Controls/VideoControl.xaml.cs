using parking_detector.Classes;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace parking_detector.Controls
{
    /// <summary>
    /// Логика взаимодействия для VideoControl.xaml
    /// </summary>
    public partial class VideoControl : UserControl
    {
        private DispatcherTimer timerVideoPlayback;

        private readonly DrawingVisual visual = new DrawingVisual();
        private RenderTargetBitmap bitmap;

        private Detection detect = new Detection();
        public VideoControl()
        {
            InitializeComponent();
        }

        private void TimerVideoPlayback_Tick(object sender, object e)
        {
            long currentMediaTicks = videoPlayer.Position.Ticks;
            long totalMediaTicks = videoPlayer.NaturalDuration.TimeSpan.Ticks;

            if (totalMediaTicks > 0)
                slider.Value = (double)currentMediaTicks / totalMediaTicks * 100;
            else
                slider.Value = 0;
        }

        private void VideoConrol_MediaOpened(object sender, RoutedEventArgs e)
        {
            timerVideoPlayback = new DispatcherTimer();
            timerVideoPlayback.Interval = TimeSpan.FromMilliseconds(10);
            timerVideoPlayback.Tick += TimerVideoPlayback_Tick;
            timerVideoPlayback.Tick += OnTimerTick;
            timerVideoPlayback.Start();
        }

        private void videoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            timerVideoPlayback.Stop();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            var width = videoPlayer.NaturalVideoWidth;
            var height = videoPlayer.NaturalVideoHeight;


            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(
                    new VisualBrush(videoPlayer), null,
                    new Rect(0, 0, width, height));
            }

            bitmap = new RenderTargetBitmap(
                width, height, 96, 96, PixelFormats.Default);


            imageResult.Source = bitmap;
            bitmap.Render(visual);


            detect.SetImage(imageResult.Source);
            detect.PreprocessImage();
            detect.RunInference();

            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(detect.ViewPrediction());
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();
            imageResult.Source = biImg as ImageSource;

        }
    }
}
