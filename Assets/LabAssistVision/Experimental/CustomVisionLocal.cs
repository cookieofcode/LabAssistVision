using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;
using Debug = UnityEngine.Debug;
using Rect = OpenCVForUnity.CoreModule.Rect;

namespace LabAssistVision
{
    /// <summary>
    /// Experimental. Predictions are working, but there is somethin off with the bounding boxes.
    /// Adapted from https://github.com/Syn-McJ/TFClassify-Unity-Barracuda and the Source Code provided from Custom Vision ONNX Model Export.
    /// </summary>
    public class CustomVisionLocal : IObjectDetector
    {
        [NotNull] private readonly Logger _logger;

        private NNModel modelFile;

        //public TextAsset labelsFile;
        public IWorker engine;
        private static readonly float[] Anchors = { 0.57273f, 0.677385f, 1.87446f, 2.06253f, 3.33843f, 5.47434f, 7.88282f, 3.52778f, 9.77052f, 9.16828f };

        private readonly IList<string> labels;
        private readonly int maxDetections;
        private readonly float probabilityThreshold;
        private readonly float iouThreshold;
        private const int imageInputSize = 416 * 416;

        public const int ROW_COUNT = 13;

        public const int COL_COUNT = 13;

        //public const int CHANNEL_COUNT = 125;
        public const int BOXES_PER_CELL = 5;
        public const int BOX_INFO_FEATURE_COUNT = 5;
        public const int CLASS_COUNT = 9;
        public const float CELL_WIDTH = 32;
        public const float CELL_HEIGHT = 32;

        private int channelStride = ROW_COUNT * COL_COUNT;

        public CustomVisionLocal(NNModel modelFile, TextAsset labels, int maxDetections = 20, float probabilityThreshold = 0.1f, float iouThreshold = 0.45f)
        {
            _logger = new Logger(new LogHandler());
            Assert.IsNotNull(_logger, "_logger != null");

            outputParser = new OutputParser();
            outputParser.SetLabels(Regex.Split(labels.text, "\n|\r|\r\n").Where(s => !String.IsNullOrEmpty(s)).ToArray());
            outputParser.SetColors(new Color[] { new Color(), new Color(), new Color(), new Color(), new Color(), new Color(), new Color(), new Color(), new Color() });
            var model = ModelLoader.Load(modelFile);
            //engine = WorkerFactory.CreateWorker(model, WorkerFactory.Device.GPU, false);
            engine = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);

            this.labels = labels.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            //this.labels = Regex.Split(labels.text, "\n|\r|\r\n")
            //    .Where(s => !String.IsNullOrEmpty(s)).ToList();;
            this.maxDetections = maxDetections;
            this.probabilityThreshold = probabilityThreshold;
            this.iouThreshold = iouThreshold;
        }

        // Settings for the image input
        public struct ImageNetSettings
        {
            public const int imageHeight = 416;
            public const int imageWidth = 416;
        }

        public struct ModelSettings
        {
            // for checking Model input and output parameter names,
            // you can use tools like Netron

            public const string ModelInput = "data";
            public const string ModelOutput = "model_outputs0";
        }

        // This detector is designed to read .onnx models (hopefully!!!!)
        private const int IMAGE_MEAN = 0;
        private const float IMAGE_STD = 1f;

        private OutputParser outputParser = new OutputParser();

        // Minimum detection confidence to consider a detection
        // The bigger the value the major certenty, but less matches found!
        private const float MINIMUM_CONFIDENCE = 0.2f;

        /// <summary>
        /// Call the Computer Vision Service to submit the image. This function is heavy, and
        /// will be wrapped up in a task so that it can be executed asynchronously with video pipeline
        /// and Unity UI thread.
        /// <param name="frame">The current video frame</param>
        /// </summary>
        [NotNull, ItemNotNull]
        public async Task<List<DetectedObject>> DetectAsync(CameraFrame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            Imgcodecs.imwrite(Application.persistentDataPath + "/testB.jpg", frame.Mat);

            try
            {
                Debug.Log($"Enter PredictImageAsync with mat {frame.Mat}");
                var imageWidth = frame.Width;
                var imageHeight = frame.Height;

                Mat rgb = new Mat(imageWidth, imageHeight, CvType.CV_8UC3);
                if (frame.Format == ColorFormat.Grayscale)
                {
                    Imgproc.cvtColor(frame.Mat, rgb, Imgproc.COLOR_GRAY2RGB);
                    Debug.Log($"Converted gray2rgb to {rgb}");
                }
                else
                {
                    frame.Mat.copyTo(rgb);
                }

                //Mat rgba = new Mat();
                //Imgproc.cvtColor(rgb, rgba, Imgproc.COLOR_RGB2RGBA);

                float newHeight = 416.0f / imageWidth * imageHeight;
                Mat resized = new Mat(416, 416, CvType.CV_8UC3);
                Imgproc.resize(rgb, resized, new Size(416, newHeight), 0.5, 0.5, Imgproc.INTER_LINEAR);
                //Imgproc.resize(rgb, resized, new Size(targetWidth, targetHeight), 0.5, 0.5, Imgproc.INTER_LINEAR);
                Debug.Log($"Resized {resized}");

                Mat resizedBorder = new Mat();
                Core.copyMakeBorder(resized, resizedBorder, 0, (int)(416 - newHeight), 0, 0, Core.BORDER_CONSTANT, new Scalar(0, 0, 0));

                /*Mat rgba = new Mat();
                Imgproc.cvtColor(resizedBorder, rgba, Imgproc.COLOR_RGB2RGBA);*/

                Texture2D texture = new Texture2D(416, 416, TextureFormat.RGB24, false);
                Utils.matToTexture2D(resizedBorder, texture, true);
                //texture.Apply();
                Color32[] pixels32 = texture.GetPixels32();

                byte[] encodeArrayToJPG = ImageConversion.EncodeArrayToJPG(pixels32, GraphicsFormat.R8G8B8A8_UInt, 416, 416);
                File.WriteAllBytes(Application.persistentDataPath + "/testA.jpg", encodeArrayToJPG);
                
                using (var tensor = TransformInput(pixels32, ImageNetSettings.imageWidth, ImageNetSettings.imageHeight))
                {
                    var inputs = new Dictionary<string, Tensor>();
                    inputs.Add(ModelSettings.ModelInput, tensor);
                    //yield return StartCoroutine(worker.StartManualSchedule(inputs));
                    //var output = engine.Execute(inputs).PeekOutput();
                    var output = engine.Execute(inputs).PeekOutput(ModelSettings.ModelOutput);
                    var results = outputParser.ParseOutputs(output, MINIMUM_CONFIDENCE);
                    var boxes = outputParser.FilterBoundingBoxes(results, 10, MINIMUM_CONFIDENCE);
                    foreach (var box in boxes)
                    {
                        Debug.Log($"{box.tagName}, {box.probability}, {box.boundingBox.left},{box.boundingBox.top},{box.boundingBox.width},{box.boundingBox.height},");
                    }

                    List<DetectedObject> detectedObjects = boxes.Select(prediction => CreateDetectedObject(frame, prediction, (int)newHeight)).ToList();
                    int count = 0;
                    foreach (var detectedObject in detectedObjects)
                    {
                        count++;
                        Mat clone = frame.Mat.clone();
                        Imgproc.rectangle(clone, detectedObject.Rect.tl(), detectedObject.Rect.br(), new Scalar(255, 255, 255), 10, 1, 0);
                        Imgcodecs.imwrite(Application.persistentDataPath + "/clone-" + count + ".jpg", clone);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw e;
            }

            return new List<DetectedObject>();

        }

        // Transform picture to tensor without the WinML library
        public static Tensor TransformInput(Color32[] pic, int width, int height)
        {
            float[] floatValues = new float[width * height * 3];

            for (int i = 0; i < pic.Length; ++i)
            {
                var color = pic[i];

                floatValues[i * 3 + 0] = (color.r - IMAGE_MEAN) / IMAGE_STD;
                floatValues[i * 3 + 1] = (color.g - IMAGE_MEAN) / IMAGE_STD;
                floatValues[i * 3 + 2] = (color.b - IMAGE_MEAN) / IMAGE_STD;
            }
            Debug.Log(String.Join(",",floatValues.ToArray()));

            return new Tensor(1, height, width, 3, floatValues);
        }
        
        [NotNull]
        private DetectedObject CreateDetectedObject([NotNull] CameraFrame frame, [NotNull] Prediction prediction, int newHeight)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (prediction == null) throw new ArgumentNullException(nameof(prediction));
            BoundingBox boundingBox = prediction.boundingBox;
            string label = prediction.tagName ?? "";
            double probability = prediction.probability;
            int width = frame.Width;
            int height = frame.Height;
            Rect2d rect = Convert(boundingBox, width, height, newHeight);
            return new DetectedObject(rect, label, probability, frame);
        }

        [NotNull]
        private Rect2d Convert([NotNull] BoundingBox boundingBox, int width, int height, int newHeight)
        {
            if (boundingBox == null) throw new ArgumentNullException(nameof(boundingBox));
            // https://stackoverflow.com/questions/50794707/how-to-use-azure-custom-vision-service-response-boundingbox-to-plot-shape
            // BoundingBox values are in percent of the image original size
            //return new Rect2d(boundingBox.left/416.0f * width, height-boundingBox.top/416.0f * width, boundingBox.width/416.0f * width, boundingBox.height/416.0f * width);
            //return new Rect2d(boundingBox.left/416.0f * width, (boundingBox.top-boundingBox.height)/416.0f * width, boundingBox.width/416.0f * width, boundingBox.height/416.0f * width);
            //return new Rect2d(boundingBox.left / 416.0f * width, (boundingBox.top/416.0f-boundingBox.height/416.0f) * width, boundingBox.width / 416.0f * width, boundingBox.height / 416.0f * width);
            return new Rect2d(boundingBox.left / 416.0f * width, (boundingBox.top / 416.0f - boundingBox.height / 416.0f) * newHeight, boundingBox.width / 416.0f * width, boundingBox.height / 416.0f * width);
        }
    }

    public class DimensionsBase
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }
    }

    public class BoundingBoxDimensions : DimensionsBase
    {
    }

    public class YoloBoundingBox
    {
        public BoundingBoxDimensions Dimensions { get; set; }

        public string Label { get; set; }

        public float Confidence { get; set; }

        public UnityEngine.Rect Rect
        {
            get { return new UnityEngine.Rect(Dimensions.X, Dimensions.Y, Dimensions.Width, Dimensions.Height); }
        }

        public Color BoxColor { get; set; }
    }

    class CellDimensions : DimensionsBase
    {
    }

    // adapted from https://github.com/jasr88/object-detection
    public class OutputParser
    {
        // Number of classes to detect (Tags in the model) 
        public int classCount = 9;

        // Constant Values for the ONNX model
        public const int ROW_COUNT = 13;
        public const int COL_COUNT = 13;
        public const int CHANNEL_COUNT = 125;
        public const int BOXES_PER_CELL = 5;
        public const int BOX_INFO_FEATURE_COUNT = 5;
        public const float CELL_WIDTH = 32;
        public const float CELL_HEIGHT = 32;

        // Anchors are pre-defined height and width ratios of bounding boxes.
        /*private float[] anchors = new float[]
        {
            1.08F, 1.19F, 3.42F, 4.41F, 6.63F, 11.38F, 9.42F, 5.11F, 16.62F, 10.52F
        };*/
        private static readonly float[] anchors = { 0.57273f, 0.677385f, 1.87446f, 2.06253f, 3.33843f, 5.47434f, 7.88282f, 3.52778f, 9.77052f, 9.16828f };

        // The length of this array must match with the ClassCount variable
        private string[] labels = null;

        // There are colors associated with each of the classes.
        private static Color[] colors = null;

        public void SetClassCount(int count)
        {
            classCount = count;
        }

        public void SetLabels(string[] classLabels)
        {
            labels = classLabels;
        }

        public void SetColors(Color[] classColors)
        {
            colors = classColors;
        }

        // Applies the sigmoid function that outputs a number between 0 and 1.
        private float Sigmoid(float value)
        {
            var k = (float)Math.Exp(value);
            return k / (1.0f + k);
        }

        // Normalizes an input vector into a probability distribution.
        private float[] Softmax(float[] values)
        {
            var maxVal = values.Max();
            var exp = values.Select(v => Math.Exp(v - maxVal));
            var sumExp = exp.Sum();

            return exp.Select(v => (float)(v / sumExp)).ToArray();
        }

        // Skip the "GetOffset" method due the Tensor class in Unity's barracuda supports a multi-dimension array
        // Extracts the bounding box dimensions from the model output.
        private BoundingBoxDimensions ExtractBoundingBoxDimensions(Tensor modelOutput, int x, int y, int channel)
        {
            return new BoundingBoxDimensions
            {
                X = modelOutput[0, x, y, channel],
                Y = modelOutput[0, x, y, channel + 1],
                Width = modelOutput[0, x, y, channel + 2],
                Height = modelOutput[0, x, y, channel + 3]
            };
        }

        // Extracts the confidence value that states how sure the model is that it has detected an object and uses the Sigmoid function to turn it into a percentage.
        private float GetConfidence(Tensor modelOutput, int x, int y, int channel)
        {
            return Sigmoid(modelOutput[0, x, y, channel + 4]);
        }

        // Uses the bounding box dimensions and maps them onto its respective cell within the image.
        private CellDimensions MapBoundingBoxToCell(int x, int y, int box, BoundingBoxDimensions boxDimensions)
        {
            return new CellDimensions
            {
                X = ((float)y + Sigmoid(boxDimensions.X)) * CELL_WIDTH,
                Y = ((float)x + Sigmoid(boxDimensions.Y)) * CELL_HEIGHT,
                Width = (float)Math.Exp(boxDimensions.Width) * CELL_WIDTH * anchors[box * 2],
                Height = (float)Math.Exp(boxDimensions.Height) * CELL_HEIGHT * anchors[box * 2 + 1],
            };
        }

        // Extracts the class predictions for the bounding box from the model output and turns them into a probability distribution using the Softmax method.
        public float[] ExtractClasses(Tensor modelOutput, int x, int y, int channel)
        {
            float[] predictedClasses = new float[classCount];
            int predictedClassOffset = channel + BOX_INFO_FEATURE_COUNT;

            for (int predictedClass = 0; predictedClass < classCount; predictedClass++)
            {
                predictedClasses[predictedClass] = modelOutput[0, x, y, predictedClass + predictedClassOffset];
            }

            return Softmax(predictedClasses);
        }

        // Selects the class from the list of predicted classes with the highest probability.
        private ValueTuple<int, float> GetTopResult(float[] predictedClasses)
        {
            return predictedClasses
                .Select((predictedClass, index) => (Index: index, Value: predictedClass))
                .OrderByDescending(result => result.Value)
                .First();
        }

        // Filters overlapping bounding boxes with lower probabilities.
        private float IntersectionOverUnion(UnityEngine.Rect boundingBoxA, UnityEngine.Rect boundingBoxB)
        {
            var areaA = boundingBoxA.width * boundingBoxA.height;

            if (areaA <= 0)
                return 0;

            var areaB = boundingBoxB.width * boundingBoxB.height;

            if (areaB <= 0)
                return 0;

            var minX = Math.Max(boundingBoxA.xMin, boundingBoxB.xMin);
            var minY = Math.Max(boundingBoxA.yMin, boundingBoxB.yMin);
            var maxX = Math.Min(boundingBoxA.xMax, boundingBoxB.xMax);
            var maxY = Math.Min(boundingBoxA.yMax, boundingBoxB.yMax);

            var intersectionArea = Math.Max(maxY - minY, 0) * Math.Max(maxX - minX, 0);

            return intersectionArea / (areaA + areaB - intersectionArea);
        }

        public IList<Prediction> ParseOutputs(Tensor modelTensorOutput, float threshold = 0.3F)
        {
            List<Prediction> boxes = new List<Prediction>();

            for (int row = 0; row < COL_COUNT; row++)
            {
                for (int colum = 0; colum < ROW_COUNT; colum++)
                {
                    for (int box = 0; box < BOXES_PER_CELL; box++)
                    {

                        int channel = (box * (classCount + BOX_INFO_FEATURE_COUNT));
                        BoundingBoxDimensions bbd = ExtractBoundingBoxDimensions(modelTensorOutput, colum, row, channel);
                        float confidence = GetConfidence(modelTensorOutput, colum, row, channel);

                        CellDimensions mappedBoundingBox = MapBoundingBoxToCell(colum, row, box, bbd);

                        if (confidence < threshold)
                        {
                            continue;
                        }

                        float[] predictedClasses = ExtractClasses(modelTensorOutput, colum, row, channel);
                        var (topResultIndex, topResultScore) = GetTopResult(predictedClasses);
                        var topScore = topResultScore * confidence;

                        if (topScore < threshold)
                        {
                            continue;
                        }

                        BoundingBox bb = new BoundingBox((mappedBoundingBox.X - mappedBoundingBox.Width / 2), (mappedBoundingBox.Y - mappedBoundingBox.Height / 2), mappedBoundingBox.Width, mappedBoundingBox.Height);
                        boxes.Add(new Prediction(topScore, labels[topResultIndex], bb));
                    }
                }
            }

            return boxes;
        }

        public IList<Prediction> FilterBoundingBoxes(IList<Prediction> boxes, int limit, float threshold)
        {
            var activeCount = boxes.Count;
            var isActiveBoxes = new bool[boxes.Count];

            for (int i = 0; i < isActiveBoxes.Length; i++)
            {
                isActiveBoxes[i] = true;
            }

            var sortedBoxes = boxes.Select((b, i) => new { Box = b, Index = i })
                .OrderByDescending(b => b.Box.probability)
                .ToList();

            var results = new List<Prediction>();

            for (int i = 0; i < boxes.Count; i++)
            {
                if (isActiveBoxes[i])
                {
                    var boxA = sortedBoxes[i].Box;
                    results.Add(boxA);

                    if (results.Count >= limit)
                        break;

                    for (var j = i + 1; j < boxes.Count; j++)
                    {
                        if (isActiveBoxes[j])
                        {
                            var boxB = sortedBoxes[j].Box;

                            if (IntersectionOverUnion(new UnityEngine.Rect((float)boxA.boundingBox.left, (float)boxA.boundingBox.top, (float)boxA.boundingBox.width, (float)boxA.boundingBox.height), new UnityEngine.Rect((float)boxB.boundingBox.left, (float)boxB.boundingBox.top, (float)boxB.boundingBox.width, (float)boxB.boundingBox.height)) > threshold)
                            {
                                isActiveBoxes[j] = false;
                                activeCount--;

                                if (activeCount <= 0)
                                    break;
                            }
                        }
                    }

                    if (activeCount <= 0)
                        break;
                }
            }

            return results;
        }
    }
}