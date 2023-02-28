using parking_detector.Classes.Detections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace parking_detector.Classes.Parking
{
    //Класс для работы с коллекцией парковок
    public static class ParkingController
    {
        private static List<ParkingSpace> parkingSpaces = new List<ParkingSpace>();

        public delegate void DeleteBox(UIElement rect); //Делегат метода удаления треугольника из
                                                        //из канваса

        public static DeleteBox deleteBox;

        public static Rectangle movingRect = null;
        public static Rectangle deformingRect = null;


        //проверка порковочных мест 
        //(Возможно не стоит передавать обьект детект, а 
        //ограничится коллекцие обноружений)
        public static void CheckParkingSpace(Detection detect)
        {
            int matches = 2;
            foreach (var pSpace in parkingSpaces)
            {
                var isFree = true;
                foreach (var prediction in detect.predictions)
                {
                    var ovr = Functions.IntersectionArea(pSpace.box, prediction.Box);
                    if (ovr > 0.2)
                    {
                        pSpace.Matches = pSpace.Matches == matches ? 
                            matches : ++pSpace.Matches;
                        if (pSpace.Matches >= matches)
                        {
                            pSpace.IsFree = isFree = false;
                        }
                        break;
                    }
                }
                if(isFree && pSpace.Matches >= 2)
                {
                    pSpace.IsFree = isFree;
                    pSpace.Matches = 0;
                }else if (isFree)
                    pSpace.IsFree = isFree;
            }
        }
        
        //Добавление элемента в коллекцию
        public static void AddElement(ParkingSpace pS)
        {
            parkingSpaces.Add(pS);
        }

        //Количество мест на парковке
        public static int Count()
        {
            return parkingSpaces.Count;
        }

        //Количество занятых
        public static int CountTaken()
        {
            return parkingSpaces.Where(e => !e.IsFree).Count();
        }

        //Обнуление скорее всего потом понадобится
        public static void Reset()
        {
            parkingSpaces.Clear();
        }

        //метод удаления элемента из коллекции парковок
        public static void DeleteItem(ParkingSpace pS)
        {
            deleteBox(pS.Rectangle);
            deleteBox(pS.SpaceID);
            parkingSpaces.Remove(pS);
        }

        /*public static bool IsChangeParking()
        {
            foreach(var parkingSpace in parkingSpaces)
            {
                if(parkingSpace.isMove)
                {
                    return true;
                }
            }
            return false;
        }*/


        
        public static Point GetCentreRectangle()
        {
            Point point = new Point();
            point.X = movingRect.Width - Canvas.GetLeft(movingRect);
            point.Y = movingRect.Height - Canvas.GetTop(movingRect);
            return point;
        }

        //Обновление парковочного места
        public static void UpdateParkingSpace()
        {
            var pSpace = parkingSpaces.Find(x => x.Rectangle == movingRect || x.Rectangle == deformingRect);
            pSpace.UpdateBox();
        }

        //Получение передвигаемого парковочного места
        public static ParkingSpace GetMovingParking()
        {
            return parkingSpaces.Find(x => x.Rectangle == movingRect);
        }

        //Получение деформируемого парковочного места
        public static ParkingSpace GetDeformingParking()
        {
            return parkingSpaces.Find(x => x.Rectangle == deformingRect);
        }
    }
}
