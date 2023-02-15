﻿namespace parking_detector.Classes
{
    public class Box
    {
        public float Xmin { get; set; }
        public float Ymin { get; set; }
        public float Xmax { get; set; }
        public float Ymax { get; set; }

        public Box(float xmin, float ymin, float xmax, float ymax)
        {
            Xmin = xmin;
            Ymin = ymin;
            Xmax = xmax;
            Ymax = ymax;
        }

        public float Square()
        {
            return (Xmax - Xmin) * (Ymax - Ymin);
        }

        public float Width
        {
            get { return Xmax - Xmin; }
        }

        public float Height
        {
            get { return Ymax - Ymin; }
        }

    }
}
