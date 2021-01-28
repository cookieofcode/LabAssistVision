using System;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;

namespace LabVision
{
    /// <summary>
    /// <seealso href="https://docs.opencv.org/4.5.0/d2/da2/classcv_1_1TrackerCSRT.html"/>
    /// </summary>
    public class CSRTTracker : CvTracker
    {
        public override TrackedObject Initialize(CameraFrame frame, Rect2d rect, string label)
        {
            if (frame.Format != ColorFormat.RGB) throw new ArgumentException("CSRT Tracker requires RGB format (OpenCVForUnity)");
            return base.Initialize(frame, rect, label);
        }
       
        protected override Tracker CreateTracker()
        {
            TrackerCSRT tracker = TrackerCSRT.create();
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
