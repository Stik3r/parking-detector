using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp.Drawing.Processing;

namespace parking_detector.Classes
{
    class Detection
    {
        string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        public InferenceSession session;

        Image<Rgb24> image;
        IImageFormat format;



        List<NamedOnnxValue> inputs;
        List<Prediction> predictions;
        public Detection()
        {
            string modelPath = path + "//Model/model.onnx";
            session = session = new InferenceSession(modelPath);
        }

        public void SetImage(byte[] data)
        {
            image = SixLabors.ImageSharp.Image.Load<Rgb24>(data, out format);
        }

        public void PreprocessImage()
        {
            float ratio = 800f / Math.Min(image.Width, image.Height);
            using Stream imageStream = new MemoryStream();
            image.Mutate(x => x.Resize((int)(ratio * image.Width), (int)(ratio * image.Height)));
            image.Save(imageStream, format);

            var (inputTensorName, inputNodeMetadata) = session.InputMetadata.First();
            Tensor<float> input = new DenseTensor<float>(new[] { 1, inputNodeMetadata.Dimensions[1], inputNodeMetadata.Dimensions[2], inputNodeMetadata.Dimensions[3]});
            var mean = new[] { 102.9801f, 115.9465f, 122.7717f };
            image.Mutate(x => x.Resize(inputNodeMetadata.Dimensions[3], inputNodeMetadata.Dimensions[2]));
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
                NamedOnnxValue.CreateFromTensor(inputTensorName, input)
            };
        }


        public void RunInference()
        {
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);


            var resultsArray = results.ToArray();
            float[] boxes = resultsArray[0].AsEnumerable<float>().ToArray();
            float[] confidences = resultsArray[1].AsEnumerable<float>().ToArray();
            predictions = new List<Prediction>();
            var minConfidence = 0.7f;
            for (int i = 0; i < boxes.Length - 4; i += 4)
            {
                var index = i / 4;
                if (confidences[index] >= minConfidence)
                {
                    predictions.Add(new Prediction
                    {
                        Box = new Box(boxes[i], boxes[i + 1], boxes[i + 2], boxes[i + 3]),
                        Label = "1",
                        Confidence = confidences[index]
                    });
                }
            }
        }

        public byte[] ViewPrediction()
        {
            Font font = SystemFonts.CreateFont("Arial", 16);
            foreach (var p in predictions)
            {
                image.Mutate(x =>
                {
                    x.DrawLines(Color.Red, 2f, new PointF[] {

                        new PointF(p.Box.Xmin, p.Box.Ymin),
                        new PointF(p.Box.Xmax, p.Box.Ymin),

                        new PointF(p.Box.Xmax, p.Box.Ymin),
                        new PointF(p.Box.Xmax, p.Box.Ymax),

                        new PointF(p.Box.Xmax, p.Box.Ymax),
                        new PointF(p.Box.Xmin, p.Box.Ymax),

                        new PointF(p.Box.Xmin, p.Box.Ymax),
                        new PointF(p.Box.Xmin, p.Box.Ymin)
        });
                    x.DrawText($"{p.Label}, {p.Confidence:0.00}", font, Color.White, new PointF(p.Box.Xmin, p.Box.Ymin));
                });
            }
            MemoryStream ms = new MemoryStream();
            image.Save(ms, format);
            return ms.ToArray();
        }
    }
}
