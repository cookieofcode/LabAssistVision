using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using LabAssistVision;
using Microsoft.MixedReality.Toolkit.Utilities;
using OpenCVForUnity.CoreModule;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace Microsoft.MixedReality.Toolkit.Extensions
{
    [MixedRealityExtensionService(SupportedPlatforms.WindowsStandalone | SupportedPlatforms.WindowsUniversal)]
    public class ObjectTrackingService : BaseExtensionService, IObjectTrackingService, IMixedRealityExtensionService
    {
        public List<TrackedObject> TrackedObjects => new List<TrackedObject>(_trackedObjects);
        protected IObjectTracker ObjectTracker;
        private readonly Logger _logger;
        private readonly ObjectTrackingServiceProfile _objectTrackingServiceProfile;
        private List<TrackedObject> _trackedObjects = new List<TrackedObject>();

        public ObjectTrackingService(string name, uint priority, BaseMixedRealityProfile profile) : base(name, priority, profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            _objectTrackingServiceProfile = profile as ObjectTrackingServiceProfile;
            if (_objectTrackingServiceProfile == null) throw new ArgumentNullException(nameof(_objectTrackingServiceProfile));
            Initialize(_objectTrackingServiceProfile.tracker);

            _logger = new Logger(new LogHandler());
            Assert.IsNotNull(_logger);
        }

        private void Initialize(Tracker tracker)
        {
            switch (tracker)
            {
                case Tracker.MosseTracker:
                    ObjectTracker = new MosseTracker();
                    break;
                case Tracker.KCFTracker:
                    ObjectTracker = new KCFTracker();
                    break;
                case Tracker.BoostingTracker:
                    ObjectTracker = new BoostingTracker();
                    break;
                case Tracker.CSRTTracker:
                    ObjectTracker = new CSRTTracker();
                    break;
                case Tracker.MedianFlowTracker:
                    ObjectTracker = new MedianFlowTracker();
                    break;
                case Tracker.TLDTracker:
                    ObjectTracker = new TLDTracker();
                    break;
                case Tracker.MILTracker:
                    ObjectTracker = new MILTracker();
                    break;
                default:
                    throw new ArgumentException("Tracker not implemented");
            }
        }

        public void SwitchTracker(Tracker tracker)
        {
            ObjectTracker.Reset();
            _trackedObjects = new List<TrackedObject>();
            Initialize(tracker);
        }

        public override void Reset()
        {
            ObjectTracker.Reset();
            _trackedObjects = new List<TrackedObject>();
            base.Reset();
        }

        public Vector2 unprojectionOffset => _objectTrackingServiceProfile.unprojectionOffset;

        public int FixedTrackerCount => _objectTrackingServiceProfile.fixedTrackerCount;
        public bool ForceFixedTrackerCount => _objectTrackingServiceProfile.forceFixedTrackerCount;
        public void ToggleFixedTrackerCount()
        {
            _objectTrackingServiceProfile.forceFixedTrackerCount = !_objectTrackingServiceProfile.forceFixedTrackerCount;
            Debug.Log($"Switched force fixed tracker count to {_objectTrackingServiceProfile.forceFixedTrackerCount}");
        }

        public void ChangeFixedTrackerCount(int value)
        {
            _objectTrackingServiceProfile.fixedTrackerCount = value;
        }

        private Stopwatch stopWatch = new Stopwatch();

        public List<TrackedObject> TrackSync(CameraFrame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            stopWatch.Reset();
            stopWatch.Start();
            List<TrackedObject> trackedObjects = ObjectTracker.Update(frame);
            stopWatch.Stop();
            FPSUtils.TrackTick();
            _trackedObjects = trackedObjects;
            return trackedObjects;
        }

        public List<TrackedObject> InitializeTrackers(List<DetectedObject> detectedObjects)
        {
            if (detectedObjects == null) throw new ArgumentNullException(nameof(detectedObjects));
            List<TrackedObject> trackedObjects = new List<TrackedObject>();
            if (_objectTrackingServiceProfile.forceFixedTrackerCount)
            {
                Debug.Log("Initializing trackers using fixed amount of trackers. Selecting Detection with highest probability");
                DetectedObject detectedObject = detectedObjects.OrderByDescending(o => o.Probability).FirstOrDefault();
                if (detectedObject == null)
                {
                    _logger.LogWarning("No object detected");
                    return trackedObjects;
                }

                List<DetectedObject> clones = new List<DetectedObject>();
                for (int i = 0; i < FixedTrackerCount; i++)
                {
                    clones.Add(new DetectedObject(detectedObject));
                }
                trackedObjects.AddRange(clones.Select(InitializeTracker));
            }
            else
            {
                trackedObjects.AddRange(detectedObjects.Select(InitializeTracker));
            }
            _trackedObjects = trackedObjects;
            return trackedObjects;
        }

        [NotNull]
        private TrackedObject InitializeTracker([NotNull] CameraFrame frame, [NotNull] Rect2d rect, [NotNull] string label)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (rect == null) throw new ArgumentNullException(nameof(rect));
            if (label == null) throw new ArgumentNullException(nameof(label));
            return ObjectTracker.Initialize(frame, rect, label);
        }

        [NotNull]
        private TrackedObject InitializeTracker([NotNull] DetectedObject detectedObject)
        {
            if (detectedObject == null) throw new ArgumentNullException(nameof(detectedObject));
            return InitializeTracker(detectedObject.Frame, detectedObject.Rect, detectedObject.Label);
        }
    }
}
