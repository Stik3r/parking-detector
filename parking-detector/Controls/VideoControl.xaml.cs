using parking_detector.Classes;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.IO;

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
        private Image detections = new Image();

        private Channel<int> channel = Channel.CreateBounded<int>(1);

        public Detection detect = new Detection();
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
            timerVideoPlayback.Interval = TimeSpan.FromMilliseconds(100);
            timerVideoPlayback.Tick += TimerVideoPlayback_Tick;
            timerVideoPlayback.Tick += OnTimerTick;
            timerVideoPlayback.Start();
            detect.NaturalSize = (videoPlayer.NaturalVideoWidth, videoPlayer.NaturalVideoHeight);
        }

        private void videoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            timerVideoPlayback.Stop();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            var width = videoPlayer.NaturalVideoWidth;
            var height = videoPlayer.NaturalVideoHeight;
            drawCanvas.Height = videoPlayer.ActualHeight;
            drawCanvas.Width = videoPlayer.ActualWidth;

            detect.ActualSize = ((int)videoPlayer.ActualWidth, (int)videoPlayer.ActualHeight);

            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(
                    new VisualBrush(videoPlayer), null,
                    new Rect(0, 0, width, height));
            }

            bitmap = new RenderTargetBitmap(
                width, height, 96, 96, PixelFormats.Default);

            bitmap.Render(visual);

            Task.Run(DetectionAsync);

        }

        public void DrawOnCanvas()
        {
            drawCanvas.Children.Clear();
            foreach(var p in detect.predictions)
            {
                Rectangle rect = new Rectangle();
                rect.Stroke = Brushes.Red;
                rect.Fill = Brushes.Transparent;
                rect.Width = p.Box.Width * detect.ActualSize.Item1;
                rect.Height = p.Box.Height * detect.ActualSize.Item2;
                drawCanvas.Children.Add(rect);
                Canvas.SetLeft(rect, p.Box.Xmin * detect.ActualSize.Item1);
                Canvas.SetTop(rect, p.Box.Ymin * detect.ActualSize.Item2);
            }
        }

        async private Task DetectionAsync()
        {
            if (channel.Writer.TryWrite(1))
            {
                await Dispatcher.BeginInvoke(() => detect.SetImage((ImageSource)bitmap));
                await detect.PreprocessImage();
                await detect.RunInference();
                await Dispatcher.BeginInvoke(() => this.DrawOnCanvas());
                channel.Reader.TryRead(out int val);
            }
        }
    }
}
