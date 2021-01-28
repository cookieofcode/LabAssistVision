using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LabAssistVision;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Extensions
{
    public interface IObjectTrackingService : IMixedRealityExtensionService
    {
        List<TrackedObject> TrackedObjects { get;  }
        Vector2 unprojectionOffset { get; }
        [NotNull, ItemNotNull]
        List<TrackedObject> TrackSync([NotNull] CameraFrame frame);
        [NotNull, ItemNotNull]
        List<TrackedObject> InitializeTrackers(List<DetectedObject> detectedObjects);

        void SwitchTracker(Tracker tracker);

        int FixedTrackerCount {get; }
        bool ForceFixedTrackerCount { get; }
        void ToggleFixedTrackerCount();
        void ChangeFixedTrackerCount(int value);
    }
}