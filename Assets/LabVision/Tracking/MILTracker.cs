using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.UnityUtils;

namespace LabVision
{
    public class MILTracker : CvTracker
    {
        protected override Tracker CreateTracker()
        {
            Utils.setDebugMode(true);
            TrackerMIL tracker = TrackerMIL.create();
            Utils.setDebugMode(false);
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            return tracker;
        }

        protected override bool Initialize(Tracker tracker, Mat mat, Rect2d rect)
        {
            if (tracker == null) throw new ArgumentNullException(nameof(tracker));
            if (mat == null) throw new ArgumentNullException(nameof(mat));
            if (rect == null) throw new ArgumentNullException(nameof(rect));
            Utils.setDebugMode(true);
            bool initialized = tracker.init(mat, rect);
            Utils.setDebugMode(false);
            return initialized;
        }
    }
}
