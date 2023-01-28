using Microsoft.ML.OnnxRuntime;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace parking_detector.Classes
{
    class Detection
    {
        string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        public InferenceSession session;

        Image<Rgb24> image;
        public Detection()
        {
            string modelPath = path + "//Model/model.onnx";
            string imagePath = path + "frame.png";
        }

        public void SetImage(BitmapImage bitmap)
        {
            Stream stream = bitmap.StreamSource;
            byte[] buffer = null;

            if (stream != null && stream.Length > 0)
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    buffer = br.ReadBytes((Int32)stream.Length);
                }
            }

            image = Image.Load<Rgb24>(buffer);
        }

        public void PreprocessImage()
        {
            float ratio = 800f / Math.Min(image.Width, image.Height);
            using Stream imageStream = new MemoryStream();
            image.Mutate(x => x.Resize((int)(ratio * image.Width), (int)(ratio * image.Height)));


        }
    }
}
