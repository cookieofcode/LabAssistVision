using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LabAssistVision;
using LabAssistVision.Detection;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.Assertions;


namespace Microsoft.MixedReality.Toolkit.Extensions
{
    [MixedRealityExtensionService(SupportedPlatforms.WindowsStandalone | SupportedPlatforms.WindowsUniversal)]
    public class CustomVisionObjectDetectionService : BaseExtensionService, IObjectDetectionService, IMixedRealityExtensionService
    {
        [NotNull] private readonly ObjectDetectionServiceProfile _objectDetectionServiceProfile;

        private int _concurrentCount;

        // ReSharper disable once NotNullMemberIsNotInitialized
        [NotNull] private readonly Logger _logger;

        private readonly IObjectDetector _objectDetector;

        public CustomVisionObjectDetectionService(string name, uint priority, BaseMixedRealityProfile profile) : base(name, priority, profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            ObjectDetectionServiceProfile serviceProfile = profile as ObjectDetectionServiceProfile;
            if (serviceProfile == null) throw new ArgumentNullException(nameof(serviceProfile));
            _objectDetectionServiceProfile = serviceProfile;

            _logger = new Logger(new LogHandler());
            Assert.IsNotNull(_logger, "_logger != null");

            if (_objectDetectionServiceProfile.useLocalModel)
            {
                _objectDetector = new CustomVisionLocal(_objectDetectionServiceProfile.modelFile, _objectDetectionServiceProfile.labels,10,0.5f);
            }
            else
            {
                _objectDetector = new CustomVision(_objectDetectionServiceProfile.predictionApi, _objectDetectionServiceProfile.predictionKey);
            }
            
        }

        public async Task<List<DetectedObject>> DetectAsync(CameraFrame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (_concurrentCount > _objectDetectionServiceProfile.maxConcurrentRequestLimit) return null;
            Interlocked.Increment(ref _concurrentCount);
            List<DetectedObject> detectedObjects = await _objectDetector.DetectAsync(frame);
            detectedObjects = detectedObjects.Where(detectedObject => detectedObject.Probability > _objectDetectionServiceProfile.minimalPredictionProbability).ToList();
            detectedObjects.ForEach((detectedObject) => _logger.Log($"Detected Object: {detectedObject}"));
            Interlocked.Decrement(ref _concurrentCount);
            return detectedObjects;
        }

        public void ChangeMinimalPredictionProbability(double value)
        {
            _objectDetectionServiceProfile.minimalPredictionProbability = value;
        }

        public void ChangeMaxConcurrentRequestLimit(int value)
        {
            if (_concurrentCount > 0) _logger.LogError("Changing limit during execution is not yet supported.");
            _objectDetectionServiceProfile.maxConcurrentRequestLimit = value;
        }

        public double minimalPredictionProbability => _objectDetectionServiceProfile.minimalPredictionProbability;
        public int maxConcurrentRequests => _objectDetectionServiceProfile.maxConcurrentRequestLimit;
        public bool detectOnRepeat => _objectDetectionServiceProfile.detectOnRepeat;
        public void ToggleDetectOnRepeat()
        {
            _objectDetectionServiceProfile.detectOnRepeat = !_objectDetectionServiceProfile.detectOnRepeat;
            Debug.Log($"Changed detect on repeat to {detectOnRepeat}");
        }
    }
}
