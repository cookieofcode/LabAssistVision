using System;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;
using UnityEngine;

namespace LabAssistVision
{
    /// <summary>
    /// <seealso href="https://docs.opencv.org/4.5.0/d0/d02/classcv_1_1TrackerMOSSE.html"/>
    /// </summary>
    public class MosseTracker : CvTracker
    {

        public override TrackedObject Initialize(CameraFrame frame, Rect2d rect, string label)
        {
            if (frame.Format != ColorFormat.Grayscale) Debug.LogWarning("MOSSE tracker requires grayscale format");
            return base.Initialize(frame, rect, label);
        }
        protected override Tracker CreateTracker()
        {
            TrackerMOSSE tracker = TrackerMOSSE.create();
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
