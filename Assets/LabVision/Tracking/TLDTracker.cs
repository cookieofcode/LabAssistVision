using System;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;
using UnityEngine;

namespace LabVision
{
    /// <summary>
    /// <seealso href="https://docs.opencv.org/4.5.0/dc/d1c/classcv_1_1TrackerTLD.html"/>
    /// </summary>
    public class TLDTracker : CvTracker
    {

        public override TrackedObject Initialize(CameraFrame frame, Rect2d rect, string label)
        {
            if (frame.Format != ColorFormat.Grayscale) Debug.LogWarning("TLD Tracker works with grayscale, but the configured color format is RGB");
            return base.Initialize(frame, rect, label);
        }
        protected override Tracker CreateTracker()
        {
            TrackerTLD tracker = TrackerTLD.create();
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
