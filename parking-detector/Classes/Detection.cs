using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;


using Color = SixLabors.ImageSharp.Color;
using Font = SixLabors.Fonts.Font;
using PointF = SixLabors.ImageSharp.PointF;
using SystemFonts = SixLabors.Fonts.SystemFonts;

namespace parking_detector.Classes
{
    class Detection
    {
        string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        public InferenceSession session;

        public Image<Rgb24> image;

        (string inputTensorName, NodeMetadata inputNodeMetadata) data;

        List<NamedOnnxValue> inputs;
        List<Prediction> predictions;
        public Detection()
        {
            string modelPath = path + "//Model/model.onnx";
            session = new InferenceSession(modelPath);
            data.inputTensorName = session.InputMetadata.First().Key;
            data.inputNodeMetadata = session.InputMetadata.First().Value;
        }

        public void SetImage(ImageSource bmpSource)
        {
            using(MemoryStream ms = new MemoryStream())
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmpSource as BitmapSource));
                encoder.Save(ms);
                image = (Image<Rgb24>)Image.Load(ms.ToArray());
            }
        }

        public void PreprocessImage()
        {
            float ratio = 800f / Math.Min(image.Width, image.Height);
            using Stream imageStream = new MemoryStream();
            image.Mutate(x => x.Resize((int)(ratio * image.Width), (int)(ratio * image.Height)));
            image.SaveAsJpeg(imageStream); 


            Tensor<float> input = new DenseTensor<float>(new[] { 1, data.inputNodeMetadata.Dimensions[1],
                data.inputNodeMetadata.Dimensions[2], data.inputNodeMetadata.Dimensions[3]});
            var mean = new[] { 102.9801f, 115.9465f, 122.7717f };
            image.Mutate(x => x.Resize(data.inputNodeMetadata.Dimensions[3], 
                data.inputNodeMetadata.Dimensions[2]));


            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgb24> pixelSpan = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        input[0, 0, y, x] = pixelSpan[x].B - mean[0];
                        input[0, 1, y, x] = pixelSpan[x].G - mean[1];
                        input[0, 2, y, x] = pixelSpan[x].R - mean[2];
                    }
                }
            });

            inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(data.inputTensorName, input)
            };
        }


        public void RunInference()
        {
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);


            var resultsArray = results.ToArray();
            var boxes = resultsArray[0].AsTensor<float>().ToDenseTensor();
            var confidences = resultsArray[1].AsTensor<float>().ToDenseTensor();
            predictions = new List<Prediction>();
            var minConfidence = 0.7f;


            var maxElems = new Dictionary<int, (int, float)>();
            for(int i = 0; i < confidences.Dimensions[1]; i++)
            {
                float max = -1000;
                int maxIndx = -1;
                for(int j = 1; j < confidences.Dimensions[2]; j++)
                {
                    if(max < confidences[0, i, j] && confidences[0, i, j] > minConfidence)
                    {
                        max = confidences[0, i, j];
                        maxIndx = j;
                    }
                }
                if(maxIndx != -1)
                {
                    maxElems.Add(i, (maxIndx, max));
                }
            }

            foreach(var elem in maxElems)
            {
                Prediction p = new Prediction()
                {
                    Box = new Box(boxes[0, elem.Key, 0, 0], boxes[0, elem.Key, 0, 1],
                    boxes[0, elem.Key, 0, 2], boxes[0, elem.Key, 0, 3]),
                    Label = LabelMap.Labels[elem.Value.Item1],
                    Confidence = elem.Value.Item2
                };
                predictions.Add(p);
            }
        }

        public byte[] ViewPrediction()
        {
            Font font = SystemFonts.CreateFont("Arial", 16);
            float h = image.Height;
            float w = image.Width;
            foreach (var p in predictions)
            {
                image.Mutate(x =>
                {
                    x.DrawLines(Color.Red, 2f, new PointF[] {

                        new PointF(p.Box.Xmin * w, p.Box.Ymin * h),
                        new PointF(p.Box.Xmax * w, p.Box.Ymin * h),

                        new PointF(p.Box.Xmax * w, p.Box.Ymin * h),
                        new PointF(p.Box.Xmax * w, p.Box.Ymax * h),

                        new PointF(p.Box.Xmax * w, p.Box.Ymax * h),
                        new PointF(p.Box.Xmin * w, p.Box.Ymax * h),

                        new PointF(p.Box.Xmin * w, p.Box.Ymax * h),
                        new PointF(p.Box.Xmin * w, p.Box.Ymin * h)
        });
                    x.DrawText($"{p.Label}, {p.Confidence:0.00}", font, Color.White, new PointF(p.Box.Xmin * w, p.Box.Ymin * h));
                });
            }
            MemoryStream ms = new MemoryStream();
            image.SaveAsJpeg(ms);
            image.SaveAsJpeg("Data.jpeg");
            byte[] result = ms.ToArray();
            ms.Close();
            return result;
        }
    }
}
