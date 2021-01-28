using System;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;
using UnityEngine;

namespace LabVision
{
    /// <summary>
    /// <seealso href="https://docs.opencv.org/4.5.0/d1/d1a/classcv_1_1TrackerBoosting.html"/>
    /// </summary>
    public class BoostingTracker : CvTracker
    {
        public override TrackedObject Initialize(CameraFrame frame, Rect2d rect, string label)
        {
            if (frame.Format != ColorFormat.Grayscale) Debug.LogWarning("Boosting Tracker uses Grayscale");
            return base.Initialize(frame, rect, label);
        }

        protected override Tracker CreateTracker()
        {
            TrackerBoosting tracker = TrackerBoosting.create();
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            return tracker;
        }

        protected override bool Initialize(Tracker tracker, Mat mat, Rect2d rect)
        {
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            if (mat == null) throw new ArgumentNullException(nameof(mat));
            if (rect == null) throw new ArgumentNullException(nameof(rect));
            return tracker.init(mat, rect);
        }
    }
}
