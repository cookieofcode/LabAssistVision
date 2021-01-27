using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;
using UnityEngine;

namespace LabVision
{
    public abstract class CvTracker : IObjectTracker
    {
        /// <summary>
        /// The tracked objects with their trackers.
        /// </summary>
        [NotNull] protected List<CvTrackedObject> CvTrackedObjects = new List<CvTrackedObject>();

        public virtual TrackedObject Initialize(CameraFrame frame, Rect2d rect, string label)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (rect == null) throw new ArgumentNullException(nameof(rect));
            if (label == null) throw new ArgumentNullException(nameof(label));

            Tracker tracker = CreateTracker();
            bool initialized = Initialize(tracker, frame.Mat, rect);
            if (!initialized) Debug.LogError("tracker init fail");

            TrackedObject trackedObject = new TrackedObject(rect, label, frame.Intrinsic, frame.Extrinsic, frame.Height);
            CvTrackedObject cvTrackedObject = new CvTrackedObject(trackedObject, tracker);
            CvTrackedObjects.Add(cvTrackedObject);

            Debug.Log($"Initialized Tracker with status: {initialized}");
            return trackedObject;
        }

        [NotNull]
        protected abstract Tracker CreateTracker();

        [NotNull, ItemNotNull]
        public virtual List<TrackedObject> Update(CameraFrame frame)
        {
            if (frame == null) throw new ArgumentNullException(nameof(frame));
            if (CvTrackedObjects.Count == 0) return new List<TrackedObject>();
            List<CvTrackedObject> outdated = new List<CvTrackedObject>();
            object _lock = new object();

            Parallel.ForEach(
                CvTrackedObjects, (cvTrackedObject) =>
                {
                    cvTrackedObject.Update(frame);
                    if (!cvTrackedObject.IsOutdated()) return;
                    lock (_lock)
                    {
                        outdated.Add(cvTrackedObject);
                        Debug.Log($"Tracked object {cvTrackedObject.GetLabel()} ist outdated.");
                    }
                });

            foreach (CvTrackedObject cvTrackedObject in outdated)
            {
                CvTrackedObjects.Remove(cvTrackedObject);
            }

            return CvTrackedObjects.Select(cvTrackedObject => cvTrackedObject.GetTrackedObject()).ToList();
        }

        protected abstract bool Initialize([NotNull] Tracker tracker, [NotNull] Mat mat, [NotNull] Rect2d rect);

        public void Reset()
        {
            CvTrackedObjects = new List<CvTrackedObject>();
        }
    }
}