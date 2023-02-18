using parking_detector.Classes.Detections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Shapes;

namespace parking_detector.Classes.Parking
{
    //Класс для работы с коллекцией парковок
    public static class ParkingController
    {
        private static List<ParkingSpace> parkingSpaces = new List<ParkingSpace>();

        public delegate void DeleteBox(Rectangle rect); //Делегат метода удаления треугольника из
                                                        //из канваса

        public static DeleteBox deleteBox;


        //проверка порковочных мест 
        //(Возможно не стоит передавать обьект детект, а 
        //ограничится коллекцие обноружений)
        public static void CheckParkingSpace(Detection detect)
        {
            foreach (var pSpace in parkingSpaces)
            {
                var isFree = true;
                foreach (var prediction in detect.predictions)
                {
                    var ovr = Functions.IntersectionArea(pSpace.box, prediction.Box);
                    if (ovr > 0.2)
                    {
                        pSpace.IsFree = isFree = false;
                        break;
                    }
                }
                if (isFree)
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
            parkingSpaces.Remove(pS);
        }
    }
}
