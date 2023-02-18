using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace parking_detector.Classes.Parking
{
    public class ParkingSpace
    {
        static int id = 0;

        Rectangle rectangle; //прямоугольник на видео
        Point firstMousePoint;
        bool isFree = true;

        public Box box; //Бокс парковки
                        //Необходима для сравнения с боксами детекций

        public delegate void Operation(ParkingSpace pS); //делегат метода удаления элемента из 
                                                        //коллекции пароковок
        public Operation deleteParkingSpace;

        public Rectangle Rectangle { get => rectangle; }

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


            //Создание конеткстного меню для удаления коробки
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem()
            {
                Header = "Удалить"
            };
            menuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                deleteParkingSpace(this);
            };
            contextMenu.Items.Add(menuItem);
            rectangle.ContextMenu = contextMenu;
        }

        //Отрисовка бокса пользователем
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
                rectangle.PreviewMouseDown += OnMouseDown;
                return true;
            }
            return false;
        }

        //Событие нажатия на бокс правой кнопкой мыши
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Right)
            {
                rectangle.ContextMenu.IsOpen = true;
            }
        }
    }
}
