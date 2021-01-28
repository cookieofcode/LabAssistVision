using System;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;

namespace LabVision
{
    /// <summary>
    /// <seealso href="https://docs.opencv.org/4.5.0/d0/d26/classcv_1_1TrackerMIL.html"/>
    /// </summary>
    public class MILTracker : CvTracker
    {
        protected override Tracker CreateTracker()
        {
            TrackerMIL tracker = TrackerMIL.create();
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            return tracker;
        }

        protected override bool Initialize(Tracker tracker, Mat mat, Rect2d rect)
        {
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            if (mat == null) throw new ArgumentNullException(nameof(mat));
            if (rect == null) throw new ArgumentNullException(nameof(rect));
            bool initialized = tracker.init(mat, rect);
            return initialized;
        }
    }
}
