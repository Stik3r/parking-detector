using System;

namespace parking_detector.Classes
{
    //Класс функций для разных операций
    public static class Functions
    {
        //Сравнение боксов
        public static float IntersectionArea(Box b1, Box b2)
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
