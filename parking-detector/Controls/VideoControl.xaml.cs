using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;
using parking_detector.Classes;

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

            if (width > 0 && height > 0)
            {
                if (bitmap == null ||
                    bitmap.PixelWidth != width ||
                    bitmap.PixelHeight != height)
                {
                    using (var dc = visual.RenderOpen())
                    {
                        dc.DrawRectangle(
                            new VisualBrush(videoPlayer), null,
                            new Rect(0, 0, width, height));
                    }

                    bitmap = new RenderTargetBitmap(
                        width, height, 96, 96, PixelFormats.Default);


                    Image image = new Image();
                    image.Source = bitmap;
                    detect.SetImage((BitmapImage)image.Source);
                }

                bitmap.Render(visual);
            }
        }
    }
}
