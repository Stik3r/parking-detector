using parking_detector.Classes.Detections;
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
        private ParkingSpace drawingRect;
        private List<ParkingSpace> parkingSpace = new List<ParkingSpace>();
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
            Rectangle r = new Rectangle();
            drawingRect = new ParkingSpace(Mouse.GetPosition(canvas), r);
            canvas.Children.Add(r);
        }

        private void CanvasOnMouseMove(object sender, MouseEventArgs e)
        {
            if (isPaint)
            {
                drawingRect.TemporaryDrawingRectangle(Mouse.GetPosition(canvas));
            }
        }

        private void CanvasOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if(isPaint)
            {
                if (drawingRect.CheckSize())
                {
                    parkingSpace.Add(drawingRect);
                }
                else
                {
                    canvas.Children.RemoveAt(canvas.Children.Count - 1);
                }
                isPaint = false;
                drawingRect = null;
            }
        }

        //Проверка парковочных мест на занятость(желательно создать отдельный
        //класс для работы с коллекцией парковок)
        private void CheckParkingSpace()
        {
            foreach(var pSpace in parkingSpace)
            {
                var isFree = true;
                foreach(var prediction in detect.predictions)
                {
                    var ovr = Functions.IntersectionArea(pSpace.box, prediction.Box);
                    if(ovr > 0.2)
                    {
                        pSpace.IsFree = isFree = false;
                        break;
                    }
                }
                if (isFree)
                    pSpace.IsFree = isFree;
            }
        }
    }
}
