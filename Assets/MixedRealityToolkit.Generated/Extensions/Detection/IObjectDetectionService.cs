using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LabAssistVision;

namespace Microsoft.MixedReality.Toolkit.Extensions
{
    public interface IObjectDetectionService : IMixedRealityExtensionService
    {
        Task<List<DetectedObject>> DetectAsync([NotNull] CameraFrame frame);
        void ChangeMinimalPredictionProbability(double value);
        void ChangeMaxConcurrentRequestLimit(int value);
        double minimalPredictionProbability { get; }
        int maxConcurrentRequests { get; }
        bool detectOnRepeat { get; }
        void ToggleDetectOnRepeat();
    }
}