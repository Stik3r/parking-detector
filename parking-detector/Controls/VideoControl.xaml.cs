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
using System.Windows.Input;
using System.Collections.Generic;

namespace parking_detector.Controls
{
    /// <summary>
    /// Логика взаимодействия для VideoControl.xaml
    /// </summary>
    public partial class VideoControl : UserControl
    {
        private DispatcherTimer timerVideoPlayback;                     //Таймер
        private readonly DrawingVisual visual = new DrawingVisual();    //Визуал для захвата кадра
        private RenderTargetBitmap bitmap;                              //Захваченый кадр

        private Channel<int> channel = Channel.CreateBounded<int>(1);   

        private Detection detect = new Detection();                      //Класс детекции

        bool isPaint = false;
        private Point firstMousePoint;
        private Rectangle drawingRect;
        private List<Rectangle> parkingSpace = new List<Rectangle>();
        public VideoControl()
        {
            InitializeComponent();
        }

        //Ползунок времени
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
        }

        private void videoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            timerVideoPlayback.Stop();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            var width = videoPlayer.NaturalVideoWidth;
            var height = videoPlayer.NaturalVideoHeight;
            canvas.Height = videoPlayer.ActualHeight;
            canvas.Width = videoPlayer.ActualWidth;

            detect.ActualSize = ((int)videoPlayer.ActualWidth, (int)videoPlayer.ActualHeight);

            using (var dc = visual.RenderOpen())                    //
            {                                                       //
                dc.DrawRectangle(                                   //
                    new VisualBrush(videoPlayer), null,             //
                    new Rect(0, 0, width, height));                 //
            }                                                       //Захват кадра
                                                                    //
            bitmap = new RenderTargetBitmap(                        //
                width, height, 96, 96, PixelFormats.Default);       //
                                                                    //
            bitmap.Render(visual);                                  //

            Task.Run(DetectionAsync);   //Вызов детекции

        }

        //Отрисовка bbox на канвасе (Наверное уже не нужно)
        private void DrawPredictionsOnCanvas()
        {
            canvas.Children.Clear();
            foreach(var p in detect.predictions)
            {
                Rectangle rect = new Rectangle();
                rect.Stroke = Brushes.Red;
                rect.Fill = Brushes.Transparent;
                rect.Width = p.Box.Width * detect.ActualSize.Item1;
                rect.Height = p.Box.Height * detect.ActualSize.Item2;
                canvas.Children.Add(rect);
                Canvas.SetLeft(rect, p.Box.Xmin * detect.ActualSize.Item1);
                Canvas.SetTop(rect, p.Box.Ymin * detect.ActualSize.Item2);
            }
        }

        //Детекция
        private async Task DetectionAsync()
        {
            if (channel.Writer.TryWrite(1))
            {
                await Dispatcher.BeginInvoke(() => detect.SetImage((ImageSource)bitmap));
                await detect.PreprocessImage();
                await detect.RunInference();
                await Dispatcher.BeginInvoke(() => this.CheckParkingSpace());
                channel.Reader.TryRead(out int val);
            }
        }

        //Методы для рисования/удаления пользовательских квадратов
        private void CanvasOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            isPaint = true;
            firstMousePoint = Mouse.GetPosition(canvas);
            drawingRect = new Rectangle();
            canvas.Children.Add(drawingRect);
            drawingRect.Stroke = Brushes.Green;
            drawingRect.Fill = Brushes.Transparent;
            drawingRect.StrokeThickness = 2;
            Canvas.SetLeft(drawingRect, firstMousePoint.X);
            Canvas.SetTop(drawingRect, firstMousePoint.Y);
        }

        private void CanvasOnMouseMove(object sender, MouseEventArgs e)
        {
            if (isPaint)
            {
                Point secondMousePoint = Mouse.GetPosition(canvas);
                drawingRect.Width = secondMousePoint.X - firstMousePoint.X;
                drawingRect.Height = secondMousePoint.Y - firstMousePoint.Y;
            }
        }

        private void CanvasOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            isPaint = false;
            parkingSpace.Add(drawingRect);
            drawingRect = null;
        }

        private void CheckParkingSpace()
        {
            foreach(var pSpace in parkingSpace)
            {
                Box parkingBox = new Box(
                    (float)Canvas.GetLeft(pSpace),
                    (float)Canvas.GetTop(pSpace),
                    (float)(pSpace.Width + Canvas.GetLeft(pSpace)),
                    (float)(pSpace.Height + Canvas.GetTop(pSpace)));
                foreach(var prediction in detect.predictions)
                {
                    var ovr = IntersectionArea(parkingBox, prediction.Box);
                    if(ovr > 0.2)
                    {
                        pSpace.Stroke = Brushes.Red;
                        break;
                    }
                }
            }
        }

        //Код дублируется с NMS в классе Detection необходимо переделать
        private float IntersectionArea(Box b1, Box b2)
        {
            var xx1 = Math.Max(b1.Xmin, b2.Xmin);
            var xx2 = Math.Min(b1.Xmax, b2.Xmax);
            var yy1 = Math.Max(b1.Ymin, b2.Ymin);
            var yy2 = Math.Min(b1.Ymax, b2.Ymax);

            var w = Math.Max(0f, xx2 - xx1);
            var h = Math.Max(0f, yy2 - yy1);

            var inter = w * h;
            var ovr = inter / (b1.Square() + b2.Square() - inter);
            return ovr;
        }
    }
}
