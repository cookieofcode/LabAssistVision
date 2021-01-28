using System;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;

namespace LabVision
{
    /// <summary>
    /// <seealso href="https://docs.opencv.org/4.5.0/d2/dff/classcv_1_1TrackerKCF.html"/>
    /// </summary>
    public class KCFTracker : CvTracker
    {
        public override TrackedObject Initialize(CameraFrame frame, Rect2d rect, string label)
        {
            if (frame.Format != ColorFormat.RGB) throw new ArgumentException("KCF Tracker requires RGB format (OpenCVForUnity)");
            return base.Initialize(frame, rect, label);
        }

        protected override Tracker CreateTracker()
        {
            TrackerKCF tracker = TrackerKCF.create();
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
