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
    public class Detection
    {
        string path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        public InferenceSession session;

        public Image<Rgb24> image;
        public (int, int) Size { get; set; }

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
            Tensor<float> input = new DenseTensor<float>(new[] { 1, data.inputNodeMetadata.Dimensions[1],
                data.inputNodeMetadata.Dimensions[2], data.inputNodeMetadata.Dimensions[3]});
            image.Mutate(x => x.Resize(data.inputNodeMetadata.Dimensions[3], 
                data.inputNodeMetadata.Dimensions[2]));


            ToRGB(input);
            Normalize(input);

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

            var bConfidences = BestConfidences(confidences);
            MakePredictions(boxes, bConfidences);
            NMS();
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
                    x.Resize(Size.Item1, Size.Item2);
                    x.DrawLines(Color.Red, 2f, new PointF[] {

                        new PointF(p.Box.Xmin * Size.Item1, p.Box.Ymin * Size.Item2),
                        new PointF(p.Box.Xmax * Size.Item1, p.Box.Ymin * Size.Item2),
                                                          
                        new PointF(p.Box.Xmax * Size.Item1, p.Box.Ymin * Size.Item2),
                        new PointF(p.Box.Xmax * Size.Item1, p.Box.Ymax * Size.Item2),
                                                          
                        new PointF(p.Box.Xmax * Size.Item1, p.Box.Ymax * Size.Item2),
                        new PointF(p.Box.Xmin * Size.Item1, p.Box.Ymax * Size.Item2),
                                                          
                        new PointF(p.Box.Xmin * Size.Item1, p.Box.Ymax * Size.Item2),
                        new PointF(p.Box.Xmin * Size.Item1, p.Box.Ymin * Size.Item2)
        });
                    //x.DrawText($"{p.Label + ":" + predictions.IndexOf(p) + ":"}", font, Color.White, new PointF(p.Box.Xmin * w, p.Box.Ymin * h));
                });
            }
            MemoryStream ms = new MemoryStream();
            image.SaveAsJpeg(ms);
            byte[] result = ms.ToArray();
            ms.Close();
            return result;
        }

        void Normalize(Tensor<float> input)
        {
            for(int i = 0; i < input.Dimensions[1]; i++)
                for(int j = 0; j < input.Dimensions[2]; j++)
                    for(int k = 0; k < input.Dimensions[3]; k++)
                    {
                        input[0, i, j, k] /= 255f;
                    }
        }

        void ToRGB(Tensor<float> input)
        {
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgb24> pixelSpan = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        input[0, 0, y, x] = pixelSpan[x].R;
                        input[0, 1, y, x] = pixelSpan[x].G;
                        input[0, 2, y, x] = pixelSpan[x].B;
                    }
                }
            });

        }

        //Dictionary<int, (int , float)> is <idBox, (indxLabel, confValue)>
        Dictionary<int, (int , float)> BestConfidences(DenseTensor<float> confidences)
        {
            var minConfidence = 0.9f;
            var result = new Dictionary<int, (int, float)>();

            for (int i = 0; i < confidences.Dimensions[1]; i++)
            {
                float max = 0;
                int maxIndx = -1;
                for (int j = 1; j < confidences.Dimensions[2]; j++)
                {
                    if (max < confidences[0, i, j] && confidences[0, i, j] > minConfidence)
                    {
                        max = confidences[0, i, j];
                        maxIndx = j;
                    }
                }
                if (maxIndx != -1)
                {
                    result.Add(i, (maxIndx + 1, max));
                }
            }
            return result;
        }

        void MakePredictions(DenseTensor<float> boxes, Dictionary<int, (int, float)> confidences)
        {
            predictions = new List<Prediction>();
            

            foreach (var elem in confidences)
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

        void NMS()
        {
            List<float> areas = new List<float>();
            predictions.Sort((x, y) => y.Confidence.CompareTo(x.Confidence));
            foreach(var p in predictions)
            {
                areas.Add(p.Box.Square());
            }

            var tresh = 0.3f;
            var result = new List<Prediction>();
            while (predictions.Count > 0)
            {
                result.Add(predictions[0]);

                for(int j = 1; j < predictions.Count; j++)
                {
                    var xx1 = Math.Max(predictions[0].Box.Xmin, predictions[j].Box.Xmin);
                    var xx2 = Math.Min(predictions[0].Box.Xmax, predictions[j].Box.Xmax);
                    var yy1 = Math.Max(predictions[0].Box.Ymin, predictions[j].Box.Ymin);
                    var yy2 = Math.Min(predictions[0].Box.Ymax, predictions[j].Box.Ymax);

                    var w = Math.Max(0f, xx2 - xx1);
                    var h = Math.Max(0f, yy2 - yy1);

                    var inter = w * h;

                    var ovr = inter / (areas[0] + areas[j] - inter);
                    if(ovr > tresh)
                    {
                        areas.RemoveAt(j);
                        predictions.RemoveAt(j);
                        j--;
                    }
                }
                areas.RemoveAt(0);
                predictions.RemoveAt(0);
            }
            predictions = result;
        }
    }
}
    