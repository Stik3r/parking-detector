using parking_detector.Classes.Detections;
using parking_detector.Classes.Parking;
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

namespace parking_detector.Controls
{
    /// <summary>
    /// Логика взаимодействия для VideoControl.xaml
    /// </summary>
    public partial class VideoControl : UserControl
    {
        DispatcherTimer timerVideoPlayback;                     //Таймер
        readonly DrawingVisual visual = new DrawingVisual();    //Визуал для захвата кадра
        RenderTargetBitmap bitmap;                              //Захваченый кадр

        Channel<int> channel = Channel.CreateBounded<int>(1);
        Detection detect = new Detection();                      //Класс детекции

        bool isPaint = false;
        ParkingSpace drawingRect;

        
        public VideoControl()
        {
            InitializeComponent();
            ParkingController.deleteBox += DeleteBox; 
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

        //Открытие видео
        private void VideoConrol_MediaOpened(object sender, RoutedEventArgs e)
        {
            timerVideoPlayback = new DispatcherTimer();
            timerVideoPlayback.Interval = TimeSpan.FromMilliseconds(100);
            timerVideoPlayback.Tick += TimerVideoPlayback_Tick;
            timerVideoPlayback.Tick += OnTimerTick;
            timerVideoPlayback.Start();
        }

        //Остановка видео
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
            if(e.ChangedButton == MouseButton.Left)
            {
                isPaint = true;
                Rectangle r = new Rectangle();
                drawingRect = new ParkingSpace(Mouse.GetPosition(canvas), r);
                canvas.Children.Add(r);
            }
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
                    ParkingController.AddElement(drawingRect);
                    drawingRect.deleteParkingSpace += ParkingController.DeleteItem;
                }
                else
                {
                    canvas.Children.RemoveAt(canvas.Children.Count - 1);
                }
                isPaint = false;
                drawingRect = null;
            }
        }

        //Проверка парковочных мест на занятость
        private void CheckParkingSpace()
        {
            ParkingController.CheckParkingSpace(detect);
        }

        //Метод удаления из канваса
        private void DeleteBox(Rectangle rect)
        {
            canvas.Children.Remove(rect);
        }
    }
}
