using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenCVForUnity.CoreModule;
using UnityEngine;
using UnityEngine.Assertions;
using static System.String;

namespace LabVision.Detection
{
    public class CustomVision : IObjectDetector
    {
        [NotNull] private readonly Logger _logger;
        [NotNull] private readonly string _predictionKey;
        [NotNull] private readonly string _predictionEndpoint;

        public CustomVision([NotNull] string predictionApi, [NotNull] string predictionKey)
        {
            if (predictionApi == null) throw new ArgumentNullException(nameof(predictionApi));
            if (predictionKey == null) throw new ArgumentNullException(nameof(predictionKey));

            _logger = new Logger(new LogHandler());
            Assert.IsNotNull(_logger, "_logger != null");

            _predictionEndpoint = predictionApi;
            _predictionKey = predictionKey;
        }

        /// <summary>
        /// Call the Computer Vision Service to submit the image.
        /// <param name="frame">The current video frame</param>
        /// </summary>
        [ItemNotNull]
        public async Task<List<DetectedObject>> DetectAsync(CameraFrame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            const string jpgExtension = ".jpg";
            byte[] jpgEncoded = frame.EncodeImage(jpgExtension);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Prediction-Key", _predictionKey);
                HttpResponseMessage response;
                using (var content = new ByteArrayContent(jpgEncoded))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    Stopwatch stopwatch = new Stopwatch();
                    _logger.Log("Starting CustomVision Request");
                    stopwatch.Start();
                    response = await client.PostAsync(_predictionEndpoint, content);
                    stopwatch.Stop();
                    _logger.Log($"CustomVision request took {stopwatch.Elapsed.Milliseconds} milliseconds");
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                if (IsNullOrEmpty(jsonResponse))
                {
                    _logger.LogError("Empty response");
                    return new List<DetectedObject>();
                }
                ImagePredictionResult predictionResult = JsonUtility.FromJson<ImagePredictionResult>(jsonResponse);
                if (predictionResult == null)
                {
                    _logger.LogError("Could not parse response");
                    return new List<DetectedObject>();
                }
                return predictionResult.predictions.Select(prediction => CreateDetectedObject(frame, prediction)).ToList();
            }
        }

        [NotNull]
        private DetectedObject CreateDetectedObject([NotNull] CameraFrame frame, [NotNull] Prediction prediction)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (prediction == null) throw new ArgumentNullException(nameof(prediction));
            BoundingBox boundingBox = prediction.boundingBox;
            string label = prediction.tagName ?? "";
            double probability = prediction.probability;
            int width = frame.Width;
            int height = frame.Height;
            Rect2d rect = Convert(boundingBox, width, height);
            return new DetectedObject(rect, label, probability, frame);
        }

        [NotNull]
        private Rect2d Convert([NotNull] BoundingBox boundingBox, int width, int height)
        {
            if (boundingBox == null) throw new ArgumentNullException(nameof(boundingBox));
            // https://stackoverflow.com/questions/50794707/how-to-use-azure-custom-vision-service-response-boundingbox-to-plot-shape
            // BoundingBox values are in percent of the image original size
            return new Rect2d(boundingBox.left * width, boundingBox.top * height, boundingBox.width * width, boundingBox.height * height);
        }
    }
}