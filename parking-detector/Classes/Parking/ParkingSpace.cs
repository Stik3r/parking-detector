using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace parking_detector.Classes.Parking
{
    public class ParkingSpace
    {
        static int id = 0;

        TextBlock spaceID;
        Rectangle rectangle; //прямоугольник на видео
        Point firstMousePoint;
        bool isFree = true;

        //public bool isMove = false;



        public Box box; //Бокс парковки
                        //Необходима для сравнения с боксами детекций

        public delegate void Operation(ParkingSpace pS); //делегат метода удаления элемента из 
                                                        //коллекции пароковок
        public Operation deleteParkingSpace;

        public Rectangle Rectangle { get => rectangle; }
        public TextBlock SpaceID { get => spaceID; }

        public int Matches
        {
            get;
            set;
        }

        //ID парковки
        public int ID { get; }
        
        //Свободка ли парковка
        public bool IsFree
        { 
            set
            {
                if (value)
                    rectangle.Stroke = Brushes.Green;
                else
                    rectangle.Stroke = Brushes.Red;
                isFree = value;
            }
            get => isFree;
        }

        public ParkingSpace(Point firstMousePoint, Rectangle rectangle)
        {
            ID = id++;
            this.firstMousePoint = firstMousePoint;
            this.rectangle = rectangle;
            rectangle.Stroke = Brushes.Green;
            rectangle.StrokeThickness = 3;
            Canvas.SetLeft(rectangle, firstMousePoint.X);
            Canvas.SetTop(rectangle, firstMousePoint.Y);

            spaceID = new TextBlock();
            spaceID.Text = $"ID: {ID}";
            spaceID.FontWeight = FontWeights.Bold;
            spaceID.Foreground = Brushes.White;
            Canvas.SetLeft(spaceID, firstMousePoint.X);
            Canvas.SetTop(spaceID, firstMousePoint.Y);

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
                Canvas.SetLeft(spaceID, secondMousePoint.X);
                rectangle.Width = firstMousePoint.X - secondMousePoint.X;
            }
            else
            {
                rectangle.Width = secondMousePoint.X - firstMousePoint.X;
            }
            if (firstMousePoint.Y > secondMousePoint.Y)
            {
                Canvas.SetTop(rectangle, secondMousePoint.Y);
                Canvas.SetTop(spaceID, secondMousePoint.Y);
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
                UpdateBox();
                rectangle.MouseDown += OnMouseDown;
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
            else if(e.ChangedButton == MouseButton.Left)
            {
                if (isEdge(e.GetPosition(rectangle)))
                {
                    ParkingController.deformingRect = rectangle;
                }
                else
                {
                    ParkingController.movingRect = rectangle;
                }
            }
        }

        //Проверка на нажатие на угол
        private bool isEdge(Point mPoint)
        {
            bool edgeX = false;
            bool edgeY = false;
            if (mPoint.X < 5 || mPoint.X > rectangle.Width - 5)
                edgeX = true;
            if (mPoint.Y < 5 || mPoint.Y > rectangle.Height - 5)
                edgeY = true;

            return edgeX && edgeY;
        }

        //Обновление позиции коробки
        public void UpdateBox()
        {
            box = new Box(
                    (float)Canvas.GetLeft(rectangle),
                    (float)Canvas.GetTop(rectangle),
                    (float)(rectangle.Width + Canvas.GetLeft(rectangle)),
                    (float)(rectangle.Height + Canvas.GetTop(rectangle)));
        }
    }
}
