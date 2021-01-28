using System;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;
using UnityEngine;

namespace LabAssistVision
{
    /// <summary>
    /// <seealso href="https://docs.opencv.org/4.5.0/d7/d86/classcv_1_1TrackerMedianFlow.html"/>
    /// </summary>
    public class MedianFlowTracker : CvTracker
    {
        public override TrackedObject Initialize(CameraFrame frame, Rect2d rect, string label)
        {
            if (frame.Format != ColorFormat.Grayscale) Debug.LogWarning("Median Flow Tracker uses grayscale");
            return base.Initialize(frame, rect, label);
        }
        protected override Tracker CreateTracker()
        {
            TrackerMedianFlow tracker = TrackerMedianFlow.create();
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
