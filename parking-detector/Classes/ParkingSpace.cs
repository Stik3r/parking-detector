using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace parking_detector.Classes
{
    class ParkingSpace
    {
        static int id = 0;

        Rectangle rectangle; //прямоугольник на видео
        Point firstMousePoint;
        private bool isFree = true;

        public Box box; //Коробка парковки
                        //Необходима для сравнения с боксами детекций

        //ID парковки
        public int ID { get; }
        
        //Свободка ли парковка
        public bool IsFree
        { 
            set
            {
                if (value)
                    rectangle.Stroke = System.Windows.Media.Brushes.Green;
                else
                    rectangle.Stroke = System.Windows.Media.Brushes.Red;
                isFree = value;
            }
            get => isFree;
        }

        public ParkingSpace(Point firstMousePoint, Rectangle rectangle)
        {
            ID = id++;
            this.firstMousePoint = firstMousePoint;
            this.rectangle = rectangle;
            rectangle.Stroke = System.Windows.Media.Brushes.Green;
            rectangle.StrokeThickness = 2;
            Canvas.SetLeft(rectangle, firstMousePoint.X);
            Canvas.SetTop(rectangle, firstMousePoint.Y);
        }

        //Отрисовка прямоугольника пользователем
        public void TemporaryDrawingRectangle(Point secondMousePoint)
        {
            if (firstMousePoint.X > secondMousePoint.X)
            {
                Canvas.SetLeft(rectangle, secondMousePoint.X);
                rectangle.Width = firstMousePoint.X - secondMousePoint.X;
            }
            else
            {
                rectangle.Width = secondMousePoint.X - firstMousePoint.X;
            }
            if (firstMousePoint.Y > secondMousePoint.Y)
            {
                Canvas.SetTop(rectangle, secondMousePoint.Y);
                rectangle.Height = firstMousePoint.Y - secondMousePoint.Y;
            }
            else
            {
                rectangle.Height = secondMousePoint.Y - firstMousePoint.Y;
            }
        }

        //Проверка размера парковки меньше 10 тяжело удалить
        public bool CheckSize()
        {
            if(rectangle.Width > 10 && rectangle.Height > 10)
            {
                box = new Box(
                    (float)Canvas.GetLeft(rectangle),
                    (float)Canvas.GetTop(rectangle),
                    (float)(rectangle.Width + Canvas.GetLeft(rectangle)),
                    (float)(rectangle.Height + Canvas.GetTop(rectangle)));
                return true;
            }
            return false;
        }
    }
}
